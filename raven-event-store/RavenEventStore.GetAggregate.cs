using System.Threading.Tasks;
using Raven.Client.Documents.Session;

namespace Raven.EventStore;

public partial class RavenEventStore
{
    public Task<TAggregate> GetAggregateAsync<TAggregate>(IAsyncDocumentSession session, string streamId)
        where TAggregate : Aggregate
    {
        return HandleGetAggregateAsync<TAggregate>(session, streamId);
    }

    public TAggregate GetAggregate<TAggregate>(IDocumentSession session, string streamId)
        where TAggregate : Aggregate
    {
        return HandleGetAggregate<TAggregate>(session, streamId);
    }

    public async Task<TAggregate> GetAggregateAsync<TAggregate>(string streamId)
        where TAggregate : Aggregate
    {
        using (var session = OpenAsyncSession())
        {
            return await HandleGetAggregateAsync<TAggregate>(session, streamId);
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
}