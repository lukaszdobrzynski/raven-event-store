using System;
using System.Linq;
using System.Text.Json;

namespace Raven.EventStore;

public partial class RavenEventStore
{
    private Aggregate BuildAggregate<TStream>(TStream stream) where TStream : DocumentStream
    {
        var aggregateType = GetAggregateType<TStream>();
        if (aggregateType == null)
            return null;

        var instance = stream.Seed is not null
            ? (Aggregate)JsonSerializer.Deserialize(
                JsonSerializer.Serialize(stream.Seed, stream.Seed.GetType()),
                aggregateType)
            : (Aggregate)Activator.CreateInstance(aggregateType);

        instance.Build(stream);
        instance.StreamKey = stream.StreamKey;
        return instance;
    }

    private Type GetAggregateType<TStream>() where TStream : DocumentStream
    {
        var aggregateType = typeof(Aggregate<>).MakeGenericType(typeof(TStream));
        return _aggregates.SingleOrDefault(x => x.IsSubclassOf(aggregateType));
    }
}