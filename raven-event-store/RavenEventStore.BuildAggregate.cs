using System;

namespace Raven.EventStore;

public partial class RavenEventStore
{
    private Aggregate BuildAggregate<TStream>(TStream stream, Aggregate seed = null) where TStream : DocumentStream
    {
        if (_aggregatesByStream.TryGetValue(typeof(TStream), out var aggregateType) == false)
            return null;

        var instance = seed is not null
            ? (Aggregate<TStream>)seed
            : (Aggregate<TStream>)Activator.CreateInstance(aggregateType);

        instance.Rebuild(stream);
        instance.StreamKey = stream.StreamKey;
        return instance;
    }
}
