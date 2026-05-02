using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Raven.Client.Documents.Session;

namespace Raven.EventStore;

public partial class RavenEventStore
{
    public Task<TStream> CreateStreamAsync<TStream>(IAsyncDocumentSession session, IEnumerable<Event> events, CancellationToken cancellationToken = default) where TStream : DocumentStream, new()
    {
        return HandleCreateAsync<TStream>(session, null, events?.ToList(), cancellationToken);
    }
    
    public TStream CreateStream<TStream>(IDocumentSession session, IEnumerable<Event> events) where TStream : DocumentStream, new()
    {
        return HandleCreate<TStream>(session, null, events?.ToList());
    }

    public Task<TStream> CreateStreamAsync<TStream>(IAsyncDocumentSession session, string streamId, IEnumerable<Event> events, CancellationToken cancellationToken = default)
        where TStream : DocumentStream, new()
    {
        return HandleCreateAsync<TStream>(session, streamId, events?.ToList(), cancellationToken);
    }
    
    public TStream CreateStream<TStream>(IDocumentSession session, string streamId, IEnumerable<Event> events)
        where TStream : DocumentStream, new()
    {
        return HandleCreate<TStream>(session, streamId, events?.ToList());
    }
    
    public Task<TStream> CreateStreamAsync<TStream>(IAsyncDocumentSession session, params Event[] events) where TStream : DocumentStream, new()
    {
        return HandleCreateAsync<TStream>(session, null, events?.ToList(), CancellationToken.None);
    }
    
    public TStream CreateStream<TStream>(IDocumentSession session, params Event[] events) where TStream : DocumentStream, new()
    {
        return HandleCreate<TStream>(session, null, events?.ToList());
    }
    
    public Task<TStream> CreateStreamAsync<TStream>(IAsyncDocumentSession session, string streamId, params Event[] events)
        where TStream : DocumentStream, new()
    {
        return HandleCreateAsync<TStream>(session, streamId, events?.ToList(), CancellationToken.None);
    }
    
    public TStream CreateStream<TStream>(IDocumentSession session, string streamId, params Event[] events)
        where TStream : DocumentStream, new()
    {
        return HandleCreate<TStream>(session, streamId, events?.ToList());
    }
}