using System;
using System.Linq;
using Newtonsoft.Json;

namespace Raven.EventStore;

public partial class RavenEventStore
{
    internal void AddSnapshot(Type snapshot)
    {
        if (_settings.Snapshots.TryGetValue(snapshot, out _))
        {
            throw new ArgumentException($"Snapshot of type {snapshot.FullName} is already registered");
        }

        _settings.Snapshots.Add(snapshot);
    }

    private Aggregate TakeSnapshot<TStream>(TStream stream) where TStream : DocumentStream
    {
        var aggregateType = GetAggregateType(stream);
        if (aggregateType == null)
            return null;

        var instance = stream.SeedSnapshot is not null
            ? (Aggregate)JsonConvert.DeserializeObject(
                JsonConvert.SerializeObject(stream.SeedSnapshot),
                aggregateType)
            : (Aggregate)Activator.CreateInstance(aggregateType);

        instance.AggregateEvents(stream);
        return instance;
    }

    private Type GetAggregateType<TStream>(TStream stream) where TStream : DocumentStream
    {
        var aggregateType = typeof(Aggregate<>).MakeGenericType(stream.GetType());
        return _settings.Snapshots.SingleOrDefault(x => x.IsSubclassOf(aggregateType));
    }
}