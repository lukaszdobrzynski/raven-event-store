using System.Threading;
using System.Threading.Tasks;
using Raven.Client.Documents.Session;

namespace Raven.EventStore;

public partial class RavenEventStore
{
    private async Task<TAggregate> HandleGetAggregateAsync<TAggregate>(IAsyncDocumentSession session, string streamId, CancellationToken cancellationToken = default)
        where TAggregate : Aggregate
    {
        var stream = await session.Include<DocumentStream>(x => x.AggregateId)
            .LoadAsync<DocumentStream>(streamId, cancellationToken);

        if (stream is null || stream.AggregateId is null)
            return null;

        return await session.LoadAsync<TAggregate>(stream.AggregateId, cancellationToken);
    }

    private TAggregate HandleGetAggregate<TAggregate>(IDocumentSession session, string streamId)
        where TAggregate : Aggregate
    {
        var stream = session.Include<DocumentStream>(x => x.AggregateId)
            .Load<DocumentStream>(streamId);

        if (stream is null || stream.AggregateId is null)
            return null;

        return session.Load<TAggregate>(stream.AggregateId);
    }
}