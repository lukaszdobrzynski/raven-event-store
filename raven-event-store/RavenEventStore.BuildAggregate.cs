using System;
using System.Text.Json;

namespace Raven.EventStore;

public partial class RavenEventStore
{
    private Aggregate BuildAggregate<TStream>(TStream stream) where TStream : DocumentStream
    {
        if (_aggregatesByStream.TryGetValue(typeof(TStream), out var aggregateType) == false)
            return null;

        var instance = stream.Seed is not null
            ? (Aggregate<TStream>)JsonSerializer.Deserialize(
                JsonSerializer.Serialize(stream.Seed, stream.Seed.GetType()),
                aggregateType)
            : (Aggregate<TStream>)Activator.CreateInstance(aggregateType);

        instance.Rebuild(stream);
        instance.StreamKey = stream.StreamKey;
        return instance;
    }
}
