﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Raven.Client.ServerWide;
using Raven.Client.ServerWide.Operations;
using Raven.EventStore.Tests.Helpers;
using Testcontainers.RavenDb;

namespace Raven.EventStore.Tests;

public abstract class TestBase
{
    private readonly RavenDbContainer _container = new RavenDbBuilder()
        .WithImage("ravendb/ravendb:7.0-latest")
        .WithEnvironment("RAVEN_License_Eula_Accepted", "true")
        .WithEnvironment("RAVEN_Setup_Mode", "None")
        .WithEnvironment("RAVEN_Security_UnsecuredAccessAllowed", "PublicNetwork")
        .Build();
    
    protected IDocumentStore DocumentStore;
    private readonly ConcurrentDictionary<string, List<string>> _dbNames = new ();
    
    [OneTimeSetUp]
    public async Task SetUpOnceBeforeTests()
    {
        await _container.StartAsync();
        
        DocumentStore = new DocumentStore
        {
            Urls = [_container.GetConnectionString()]
        }.Initialize();
    }
    
    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        DocumentStore?.Dispose();

        await _container.StopAsync();
        await _container.DisposeAsync();
    }
    
    [TearDown]
    public async Task TearDownAfterEachTest()
    {
        if (_dbNames.TryGetValue(TestContext.CurrentContext.Test.FullName, out var names))
        {
            foreach (var name in names)
            {
                var operation = new DeleteDatabasesOperation(name, hardDelete:true);
                await DocumentStore.Maintenance.Server.SendAsync(operation);
            }
        }
    }

    protected async Task<string> CreateDatabase()
    {
        var dbName = Guid.NewGuid().ToString();
        
        if (_dbNames.TryGetValue(TestContext.CurrentContext.Test.FullName, out var names))
        {
            names.Add(dbName);
        }
        else
        {
            _dbNames.TryAdd(TestContext.CurrentContext.Test.FullName, [dbName]);
        } 
        
        var operation = new CreateDatabaseOperation(new DatabaseRecord(dbName));
        await DocumentStore.Maintenance.Server.SendAsync(operation);
        
        return dbName;
    }
    
    protected static string CreateEventStoreNameUnique() => Guid.NewGuid().ToString();

    protected async Task<string> CreateSemanticId<T>(string databaseName, string idSuffix = null)
    {
        var instance = Activator.CreateInstance<T>();
        var id = await DocumentStore.HiLoIdGenerator.GenerateDocumentIdAsync(databaseName, instance);
        return idSuffix == null ? id : $"{id}/{idSuffix}";
    }

    protected static string CreateSliceStreamNextId(string semanticId, string nextSuffix)
    {
        var parts = semanticId.Split("/");
        return $"{parts[0]}/{parts[1]}/{nextSuffix}";
    }

    protected RavenEventStoreBuilder InitEventStoreBuilder()
    {
        var name = CreateEventStoreNameUnique();
        var builder = RavenEventStoreBuilder.Init(DocumentStore)
            .WithName(name);
        
        return builder;
    }

    protected async Task<T> LoadAsync<T>(string dbName, string id)
    {
        using (var session = DocumentStore.OpenAsyncSession(dbName))
        {
            var document = await session.LoadAsync<T>(id);
            return document;
        }
    }

    protected async Task<T> LoadSingleAsync<T>(string dbName)
    {
        using (var session = DocumentStore.OpenAsyncSession(dbName))
        {
            var document = await session.Query<T>().SingleAsync();
            return document;
        }
    }

    protected async Task<List<T>> LoadAllAsync<T>(string dbName)
    {
        using (var session = DocumentStore.OpenAsyncSession(dbName))
        {
            var documents = await session.Query<T>().ToListAsync();
            return documents;
        }
    }

    protected async Task AssertNoDocumentInDb<T>(string dbName, string id)
    {
        using (var session = DocumentStore.OpenAsyncSession(dbName))
        {
            var document = await session.LoadAsync<T>(id);
            Assert.That(document, Is.Null);
        }
    }
}