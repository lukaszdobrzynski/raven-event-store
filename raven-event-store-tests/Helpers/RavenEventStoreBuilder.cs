using System;
using System.Collections.Generic;
using Raven.Client.Documents;

namespace Raven.EventStore.Tests.Helpers;

public class RavenEventStoreBuilder
{
    private readonly IDocumentStore _documentStore;
    private string Name { get; set; }
    private string DatabaseName { get; set; }
    private bool UseGlobalStreamLogging { get; set; }
    private List<Type> Aggregates { get; set; } = [];

    private RavenEventStoreBuilder(IDocumentStore documentStore)
    {
        _documentStore = documentStore;
    }
    
    public static RavenEventStoreBuilder Init(IDocumentStore documentStore) => new (documentStore);

    public RavenEventStoreBuilder WithName(string name)
    {
        Name = name;
        return this;
    }

    public RavenEventStoreBuilder WithDatabaseName(string databaseName)
    {
        DatabaseName = databaseName;
        return this;
    }

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
            options.Name = Name;
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

        return _documentStore.GetEventStore(Name);
    }
}