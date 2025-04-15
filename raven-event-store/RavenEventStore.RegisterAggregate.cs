using System;
using Raven.EventStore.Exceptions;

namespace Raven.EventStore;

public partial class RavenEventStore
{
    internal void RegisterAggregate(Type aggregate)
    {
        if (Settings.Aggregates.TryGetValue(aggregate, out _))
        {
            throw new EventStoreConfigurationException($"Aggregate of type {aggregate.FullName} is already registered");
        }

        Settings.Aggregates.Add(aggregate);
    }
}