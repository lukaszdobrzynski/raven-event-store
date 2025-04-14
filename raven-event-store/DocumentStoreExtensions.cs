using System;
using System.Collections.Generic;
using Raven.Client.Documents;
using Raven.EventStore.Exceptions;

namespace Raven.EventStore;

public static class DocumentStoreExtensions
{
    private static readonly Dictionary<string, RavenEventStore> EventStores = new ();

    public static void AddEventStore(this IDocumentStore documentStore, Action<RavenEventStoreConfigurationOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(documentStore);
        
        var options = new RavenEventStoreConfigurationOptions();
        configure(options);

        if (string.IsNullOrWhiteSpace(options.Name))
            throw new EventStoreConfigurationException($"{nameof(RavenEventStore)} {nameof(RavenEventStoreConfigurationOptions.Name)} cannot be empty)");

        if (EventStores.TryGetValue(options.Name, out _))
            throw new EventStoreConfigurationException($"{nameof(RavenEventStore)} with the {nameof(RavenEventStoreConfigurationOptions.Name)} {options.Name} has already been added.");
                
        var eventStore = new RavenEventStore(documentStore);
        EventStores.Add(options.Name, eventStore);
        
        ConfigureAggregates(options.Name, options.Aggregates);
        ConfigureGlobalStreamLogging(options.Name, options.UseGlobalStreamLogging);
        
        var databaseName = options.DatabaseName ?? documentStore.Database;
        ConfigureDatabaseName(options.Name, databaseName);
    }
    
    public static RavenEventStore GetEventStore(this IDocumentStore _, string name)
    {
        if (EventStores.TryGetValue(name, out var eventStore) == false)
            throw new NonExistentEventStoreException(name);
        
        return eventStore;
    }

    private static void ConfigureAggregates(string storeName, RavenEventStoreAggregates aggregates)
    {
        foreach (var type in aggregates.Types)
        {
            var eventStore = EventStores[storeName];
            eventStore.RegisterAggregate(type);
        }
    }

    private static void ConfigureGlobalStreamLogging(string storeName, bool use)
    {
        var eventStore = EventStores[storeName];
        eventStore.SetUseGlobalStreamLogging(use);  
    }

    private static void ConfigureDatabaseName(string storeName, string databaseName)
    {
        var eventStore = EventStores[storeName];
        eventStore.SetDatabaseName(databaseName);
    }
}