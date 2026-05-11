using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Raven.Client.Documents.Session;

namespace Raven.EventStore;

public partial class RavenEventStore
{
    public Task<TStream> SliceStreamAsync<TStream>(IAsyncDocumentSession session, Guid streamKey, string newStreamId,
        params Event[] events) where TStream : DocumentStream, new()
    {
        return HandleSliceAsync<TStream>(session, streamKey, newStreamId, events?.ToList(), CancellationToken.None);
    }

    public TStream SliceStream<TStream>(IDocumentSession session, Guid streamKey, string newStreamId, params Event[] events)
        where TStream : DocumentStream, new()
    {
        return HandleSlice<TStream>(session, streamKey, newStreamId, events?.ToList());
    }

    public Task<TStream> SliceStreamAsync<TStream>(IAsyncDocumentSession session, Guid streamKey, string newStreamId, IEnumerable<Event> events, CancellationToken cancellationToken = default) where TStream : DocumentStream, new()
    {
        return HandleSliceAsync<TStream>(session, streamKey, newStreamId, events?.ToList(), cancellationToken);
    }

    public TStream SliceStream<TStream>(IDocumentSession session, Guid streamKey, string newStreamId, IEnumerable<Event> events)
        where TStream : DocumentStream, new()
    {
        return HandleSlice<TStream>(session, streamKey, newStreamId, events?.ToList());
    }

    public Task<TStream> SliceStreamAsync<TStream>(IAsyncDocumentSession session, Guid streamKey,
        params Event[] events) where TStream : DocumentStream, new()
    {
        return HandleSliceAsync<TStream>(session, streamKey, newStreamId: null, events?.ToList(), CancellationToken.None);
    }

    public TStream SliceStream<TStream>(IDocumentSession session, Guid streamKey, params Event[] events)
        where TStream : DocumentStream, new()
    {
        return HandleSlice<TStream>(session, streamKey, newStreamId: null, events?.ToList());
    }

    public Task<TStream> SliceStreamAsync<TStream>(IAsyncDocumentSession session, Guid streamKey,
        IEnumerable<Event> events, CancellationToken cancellationToken = default) where TStream : DocumentStream, new()
    {
        return HandleSliceAsync<TStream>(session, streamKey, newStreamId: null, events?.ToList(), cancellationToken);
    }

    public TStream SliceStream<TStream>(IDocumentSession session, Guid streamKey, IEnumerable<Event> events)
        where TStream : DocumentStream, new()
    {
        return HandleSlice<TStream>(session, streamKey, newStreamId: null, events?.ToList());
    }
}
