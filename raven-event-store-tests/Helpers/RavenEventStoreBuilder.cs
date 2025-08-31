using System;
using System.Collections.Generic;
using Raven.Client.Documents;

namespace Raven.EventStore.Tests.Helpers;

public class RavenEventStoreBuilder
{
    private readonly IDocumentStore _documentStore;
    private string DatabaseName { get; set; }
    private bool UseGlobalStreamLogging { get; set; }
    private List<Type> Aggregates { get; set; } = [];

    private RavenEventStoreBuilder(IDocumentStore documentStore, string databaseName)
    {
        _documentStore = documentStore;
        DatabaseName = databaseName;
    }
    
    public static RavenEventStoreBuilder Init(IDocumentStore documentStore, string databaseName) => new (documentStore, databaseName);

    public RavenEventStoreBuilder WithGlobalStreamLogging()
    {
        UseGlobalStreamLogging = true;
        return this;
    }

    public RavenEventStoreBuilder WithAggregate(Type aggregate)
    {
        Aggregates.Add(aggregate);
        return this;
    }
    
    public RavenEventStore Build()
    {
        _documentStore.AddEventStore(options =>
        {
            options.DatabaseName = DatabaseName;
            options.UseGlobalStreamLogging = UseGlobalStreamLogging;
            
            options.Aggregates.Register(registry =>
            {
                foreach (var aggregate in Aggregates)
                {
                    registry.Add(aggregate);    
                }
            });
        });

        return _documentStore.GetEventStore(DatabaseName);
    }
}