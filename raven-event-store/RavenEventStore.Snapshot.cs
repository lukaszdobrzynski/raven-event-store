using System;
using System.Linq;
using System.Threading.Tasks;
using Raven.Client.Documents.Session;

namespace Raven.EventStore;

public partial class RavenEventStore
{
    internal void AddSnapshot(Type snapshot)
    {
        if (_snapshots.TryGetValue(snapshot, out _))
        {
            throw new ArgumentException($"Snapshot of type {snapshot.FullName} is already registered");
        }

        _snapshots.Add(snapshot);
    }

    private async Task TakeSnapshotAndStoreAsync<TStream>(TStream stream, IAsyncDocumentSession session) where TStream : DocumentStream
    {
        var aggregateType = GetAggregateType(stream);

        if (aggregateType is not null)
        {
            var instance = (IAggregate)Activator.CreateInstance(aggregateType);
            instance.AggregateEvents(stream);
            await session.StoreAsync(instance);
        };
    }

    private void TakeSnapshotAndStore<TStream>(TStream stream, IDocumentSession session) where TStream : DocumentStream
    {
        var aggregateType = GetAggregateType(stream);

        if (aggregateType is not null)
        {
            var instance = (IAggregate)Activator.CreateInstance(aggregateType);
            instance.AggregateEvents(stream);
            session.Store(instance);
        }
    }

    private Type GetAggregateType<TStream>(TStream stream) where TStream : DocumentStream
    {
        var aggregateType = typeof(Aggregate<>).MakeGenericType(stream.GetType());
        return _snapshots.SingleOrDefault(x => x.IsSubclassOf(aggregateType));
    }
}