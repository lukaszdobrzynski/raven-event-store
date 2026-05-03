using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Raven.Client.Documents;
using Raven.EventStore.Exceptions;

namespace Raven.EventStore;

public static class DocumentStoreExtensions
{
    private static readonly ConditionalWeakTable<IDocumentStore, ConcurrentDictionary<string, RavenEventStore>> Registries = new();

    public static void AddEventStore(this IDocumentStore documentStore, Action<RavenEventStoreConfigurationOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(documentStore);

        var options = new RavenEventStoreConfigurationOptions();
        configure(options);

        if (string.IsNullOrWhiteSpace(options.DatabaseName))
            throw new EventStoreConfigurationException($"{nameof(RavenEventStore.DatabaseName)} must be provided.");

        var eventStore = new RavenEventStore(documentStore);
        eventStore.SetDatabaseName(options.DatabaseName);
        ConfigureAggregates(eventStore, options.Aggregates);
        ConfigureGlobalStreamLogging(eventStore, options.UseGlobalStreamLogging);

        var registry = Registries.GetValue(documentStore, _ => new ConcurrentDictionary<string, RavenEventStore>(StringComparer.OrdinalIgnoreCase));

        if (registry.TryAdd(options.DatabaseName, eventStore) == false)
            throw new EventStoreConfigurationException($"{nameof(RavenEventStore)} with the database name {options.DatabaseName} has already been configured. " +
                                                       $"You cannot configure another event store accessing the same database.");
    }

    public static IRavenEventStore GetEventStore(this IDocumentStore documentStore, string databaseName)
    {
        ArgumentNullException.ThrowIfNull(documentStore);

        if (Registries.TryGetValue(documentStore, out var registry) && registry.TryGetValue(databaseName, out var eventStore))
            return eventStore;

        throw new NonExistentEventStoreException(databaseName);
    }

    private static void ConfigureAggregates(RavenEventStore eventStore, RavenEventStoreAggregates aggregates)
    {
        foreach (var type in aggregates.Registry.Types)
            eventStore.RegisterAggregate(type);
    }

    private static void ConfigureGlobalStreamLogging(RavenEventStore eventStore, bool use) =>
        eventStore.SetUseGlobalStreamLogging(use);
}
