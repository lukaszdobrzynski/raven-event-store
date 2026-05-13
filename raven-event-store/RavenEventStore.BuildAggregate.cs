using System;
using System.Collections.Generic;

namespace Raven.EventStore;

public partial class RavenEventStore
{
    private Aggregate BuildAggregate<TStream>(TStream stream, Aggregate seed = null) where TStream : DocumentStream
    {
        if (_aggregatesByStream.TryGetValue(typeof(TStream), out var aggregateType) == false)
            return null;

        var instance = seed ?? (Aggregate)Activator.CreateInstance(aggregateType);

        instance.ApplyEvents(stream.Events);
        instance.StreamKey = stream.StreamKey;
        return instance;
    }

    private static void ApplyNewEvents(Aggregate existing, IEnumerable<Event> newEvents)
    {
        existing.ApplyEvents(newEvents);
    }
}
