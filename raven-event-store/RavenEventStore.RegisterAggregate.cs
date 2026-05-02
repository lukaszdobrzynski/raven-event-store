using System;
using Raven.EventStore.Exceptions;

namespace Raven.EventStore;

public partial class RavenEventStore
{
    internal void RegisterAggregate(Type aggregate)
    {
        var streamType = aggregate.BaseType.GetGenericArguments()[0];

        if (_aggregatesByStream.TryGetValue(streamType, out var existing))
            throw new EventStoreConfigurationException(
                $"Aggregate of type {existing.FullName} is already registered for stream {streamType.FullName}.");

        _aggregatesByStream[streamType] = aggregate;
    }
}