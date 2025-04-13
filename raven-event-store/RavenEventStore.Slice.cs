using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Raven.Client.Documents.Session;

namespace Raven.EventStore;

public partial class RavenEventStore
{
    public Task<TStream> SliceStreamAsync<TStream>(IAsyncDocumentSession session, string sourceStreamId, string newStreamId, IEnumerable<Event> events) where TStream : DocumentStream, new()
    {
        return HandleSliceAsync<TStream>(session, sourceStreamId, newStreamId, events?.ToList());
    }
    
    public TStream SliceStream<TStream>(IDocumentSession session, string sourceStreamId, string newStreamId, IEnumerable<Event> events)
        where TStream : DocumentStream, new()
    {
        return HandleSlice<TStream>(session, sourceStreamId, newStreamId, events?.ToList());
    }
}