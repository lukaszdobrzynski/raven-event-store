using System;
using System.Threading;
using System.Threading.Tasks;
using Raven.Client.Documents.Session;

namespace Raven.EventStore;

public partial class RavenEventStore
{
    public Task<TAggregate> GetAggregateAsync<TAggregate>(IAsyncDocumentSession session, string streamId, CancellationToken cancellationToken = default)
        where TAggregate : Aggregate
    {
        return HandleGetAggregateAsync<TAggregate>(session, streamId, cancellationToken);
    }

    public TAggregate GetAggregate<TAggregate>(IDocumentSession session, string streamId)
        where TAggregate : Aggregate
    {
        return HandleGetAggregate<TAggregate>(session, streamId);
    }

    public async Task<TAggregate> GetAggregateAsync<TAggregate>(string streamId, CancellationToken cancellationToken = default)
        where TAggregate : Aggregate
    {
        using (var session = OpenAsyncSession())
        {
            return await HandleGetAggregateAsync<TAggregate>(session, streamId, cancellationToken);
        }
    }

    public TAggregate GetAggregate<TAggregate>(string streamId)
        where TAggregate : Aggregate
    {
        using (var session = OpenSession())
        {
            return HandleGetAggregate<TAggregate>(session, streamId);
        }
    }

    public async Task<TAggregate> GetAggregateAtVersionAsync<TAggregate, TStream>(string streamId, int version,
        CancellationToken cancellationToken = default)
        where TAggregate : Aggregate
        where TStream : DocumentStream
    {
        using (var session = OpenAsyncSession())
        {
            return await HandleGetAggregateAtVersionAsync<TAggregate, TStream>(session, streamId, version, cancellationToken);
        }
    }

    public TAggregate GetAggregateAtVersion<TAggregate, TStream>(string streamId, int version)
        where TAggregate : Aggregate
        where TStream : DocumentStream
    {
        using (var session = OpenSession())
        {
            return HandleGetAggregateAtVersion<TAggregate, TStream>(session, streamId, version);
        }
    }

    public Task<TAggregate> GetAggregateAtAsync<TAggregate>(string streamId, DateTime timestamp,
        CancellationToken cancellationToken = default) where TAggregate : Aggregate
    {
        throw new NotImplementedException();
    }

    public TAggregate GetAggregateAt<TAggregate>(string streamId, DateTime timestamp) where TAggregate : Aggregate
    {
        throw new NotImplementedException();
    }
}