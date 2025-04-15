using System;
using System.Linq;
using Newtonsoft.Json;

namespace Raven.EventStore;

public partial class RavenEventStore
{
    private Aggregate BuildAggregate<TStream>(TStream stream) where TStream : DocumentStream
    {
        var aggregateType = GetAggregateType<TStream>();
        if (aggregateType == null)
            return null;

        var instance = stream.Seed is not null
            ? (Aggregate)JsonConvert.DeserializeObject(
                JsonConvert.SerializeObject(stream.Seed),
                aggregateType)
            : (Aggregate)Activator.CreateInstance(aggregateType);

        instance.Build(stream);
        instance.StreamKey = stream.StreamKey;
        return instance;
    }

    private Type GetAggregateType<TStream>() where TStream : DocumentStream
    {
        var aggregateType = typeof(Aggregate<>).MakeGenericType(typeof(TStream));
        return Settings.Aggregates.SingleOrDefault(x => x.IsSubclassOf(aggregateType));
    }
}