using System.Threading;
using System.Threading.Tasks;
using Raven.Client.Documents.Session;

namespace Raven.EventStore;

public partial class RavenEventStore
{
    public Task<TStream> GetStreamAsync<TStream>(IAsyncDocumentSession session, string streamId, CancellationToken cancellationToken = default)
        where TStream : DocumentStream
    {
        return session.LoadAsync<TStream>(streamId, cancellationToken);
    }

    public TStream GetStream<TStream>(IDocumentSession session, string streamId)
        where TStream : DocumentStream
    {
        return session.Load<TStream>(streamId);
    }

    public async Task<TStream> GetStreamAsync<TStream>(string streamId, CancellationToken cancellationToken = default)
        where TStream : DocumentStream
    {
        using (var session = OpenAsyncSession())
        {
            return await session.LoadAsync<TStream>(streamId, cancellationToken);
        }
    }

    public TStream GetStream<TStream>(string streamId)
        where TStream : DocumentStream
    {
        using (var session = OpenSession())
        {
            return session.Load<TStream>(streamId);
        }
    }
}
