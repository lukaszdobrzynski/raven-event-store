using System;
using Raven.Client.Documents;

namespace Raven.EventStore;

public static class DocumentStoreExtensions
{
    private static RavenEventStore _eventStore;

    public static void ConfigureEventStore(this IDocumentStore documentStore, Action<RavenEventStoreConfigurationOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(documentStore);

        _eventStore = new RavenEventStore(documentStore);
        
        var options = new RavenEventStoreConfigurationOptions();
        configure(options);

        ConfigureAggregates(options.Aggregates);
        ConfigureGlobalStreamLogging(options.UseGlobalStreamLogging);
    }
    
    public static void ConfigureEventStore(this IDocumentStore documentStore)
    {
        ArgumentNullException.ThrowIfNull(documentStore);

        _eventStore = new RavenEventStore(documentStore);
    }

    public static RavenEventStore GetEventStore(this IDocumentStore _)
    {
        return _eventStore;
    }

    private static void ConfigureAggregates(RavenEventStoreAggregates aggregates)
    {
        foreach (var type in aggregates.Types)
        {
            _eventStore.RegisterAggregate(type);
        }
    }
    
    private static void ConfigureGlobalStreamLogging(bool use) => _eventStore.SetUseGlobalStreamLogging(use);
}