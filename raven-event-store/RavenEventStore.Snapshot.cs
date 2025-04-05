using System;
using System.Linq;

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

    private IAggregate TakeSnapshot<TStream>(TStream stream) where TStream : DocumentStream
    {
        var aggregateType = GetAggregateType(stream);

        if (aggregateType is not null)
        {
            var instance = (IAggregate)Activator.CreateInstance(aggregateType);
            instance.AggregateEvents(stream);
            return instance;
        };

        return null;
    }

    private Type GetAggregateType<TStream>(TStream stream) where TStream : DocumentStream
    {
        var aggregateType = typeof(Aggregate<>).MakeGenericType(stream.GetType());
        return _settings.Snapshots.SingleOrDefault(x => x.IsSubclassOf(aggregateType));
    }
}