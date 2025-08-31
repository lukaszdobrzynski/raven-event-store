using System;
using System.Collections.Generic;
using System.Linq;
using Raven.Client.Documents;
using Raven.EventStore.Exceptions;

namespace Raven.EventStore;

public static class DocumentStoreExtensions
{
    private static readonly List<RavenEventStore> EventStores = new ();

    public static void AddEventStore(this IDocumentStore documentStore, Action<RavenEventStoreConfigurationOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(documentStore);
        
        var options = new RavenEventStoreConfigurationOptions();
        configure(options);

        if (string.IsNullOrWhiteSpace(options.DatabaseName))
        {
            throw new EventStoreConfigurationException($"{nameof(RavenEventStore.DatabaseName)} must be provided.");
        }
        
        if (EventStoreExists(options.DatabaseName))
        {
            throw new EventStoreConfigurationException($"{nameof(RavenEventStore)} with the database name {options.DatabaseName} has already been configured. " +
                                                       $"You cannot configure another event store accessing the same database.");
        }

        var eventStore = new RavenEventStore(documentStore);
        eventStore.SetDatabaseName(options.DatabaseName);
        EventStores.Add(eventStore);
        
        ConfigureAggregates(eventStore, options.Aggregates);
        ConfigureGlobalStreamLogging(eventStore, options.UseGlobalStreamLogging);
    }
    
    public static RavenEventStore GetEventStore(this IDocumentStore _, string databaseName)
    {
        if (EventStoreExists(databaseName) == false)
            throw new NonExistentEventStoreException(databaseName);
        
        return EventStores.Single(x => x.DatabaseName.Equals(databaseName, StringComparison.OrdinalIgnoreCase));
    }

    private static void ConfigureAggregates(RavenEventStore eventStore, RavenEventStoreAggregates aggregates)
    {
        foreach (var type in aggregates.Registry.Types)
        {
            eventStore.RegisterAggregate(type);
        }
    }

    private static void ConfigureGlobalStreamLogging(RavenEventStore eventStore, bool use)
    {
        eventStore.SetUseGlobalStreamLogging(use);  
    }

    private static bool EventStoreExists(string databaseName) => EventStores.Exists(x =>
        x.DatabaseName.Equals(databaseName, StringComparison.OrdinalIgnoreCase));
}