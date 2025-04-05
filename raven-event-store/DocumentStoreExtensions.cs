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

        ConfigureProjections(options.Projections);
        ConfigureSnapshots(options.Snapshots);
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

    private static void ConfigureProjections(RavenEventStoreProjections projections)
    {
        foreach (var type in projections.Types)
        {
            _eventStore.AddProjection(type);
        }
    }

    private static void ConfigureSnapshots(RavenEventStoreSnapshots snapshots)
    {
        foreach (var type in snapshots.Types)
        {
            _eventStore.AddSnapshot(type);
        }
    }
    
    private static void ConfigureGlobalStreamLogging(bool use) => _eventStore.SetUseGlobalStreamLogging(use);
}