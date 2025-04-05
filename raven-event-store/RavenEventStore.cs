using System.Collections.Generic;
using System.Threading.Tasks;
using IdGen;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;

namespace Raven.EventStore;

public partial class RavenEventStore
{
    private readonly EventStoreSettings _settings = new();
    private static readonly IdGenerator GlobalEventLogSequentialIdGenerator = new (0);
    
    private IDocumentStore DocumentStore { get; }

    internal RavenEventStore(IDocumentStore documentStore)
    {
        DocumentStore = documentStore;
    }

    private static void AssignVersionToEvents(List<Event> events, int nextVersion)
    {
        events.ForEach(e => e.Version = nextVersion++);
    }
    
    private (IAggregate snapshot, List<IProjection> projections) RunSnapshotAndProjections<TStream>(TStream stream) where TStream : DocumentStream
    {
        var takeSnapshotTask = Task.Run(() => TakeSnapshot(stream));
        var projectionsTask = Task.Run(() => RunProjections(stream));

        Task.WhenAll(takeSnapshotTask, projectionsTask);

        return (takeSnapshotTask.Result, projectionsTask.Result);
    }
    
    private static async Task StoreSnapshotAndProjectionsAsync(IAsyncDocumentSession session, IAggregate aggregateSnapshot,
        List<IProjection> projections)
    {
        if (aggregateSnapshot != null)
        {
            await session.StoreAsync(aggregateSnapshot);
        }

        foreach (var projection in projections)
        {
            await session.StoreAsync(projection);
        }
    }
    
    private static void StoreSnapshotAndProjections(IDocumentSession session, IAggregate aggregateSnapshot, List<IProjection> projections)
    {
        if (aggregateSnapshot != null)
        {
            session.Store(aggregateSnapshot);
        }

        foreach (var projection in projections)
        {
            session.Store(projection);
        }
    }
}