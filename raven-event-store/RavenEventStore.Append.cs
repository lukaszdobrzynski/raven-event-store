using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Raven.Client.Documents.Session;

namespace Raven.EventStore;

public partial class RavenEventStore
{
    public async Task AppendAsync<TStream>(IAsyncDocumentSession session, string streamId, List<Event> events) where TStream : DocumentStream
    {
        await HandleAppendAsync<TStream>(session, streamId, events);
    }
    
    public void Append<TStream>(IDocumentSession session, string streamId, List<Event> events) where TStream : DocumentStream
    {
        HandleAppend<TStream>(session, streamId, events);
    }
    
    public async Task AppendAsync<TStream>(IAsyncDocumentSession session, string streamId, params Event[] events) where TStream : DocumentStream
    {
        await HandleAppendAsync<TStream>(session, streamId, events?.ToList());
    }
    
    public void Append<TStream>(IDocumentSession session, string streamId, params Event[] events) where TStream : DocumentStream
    {
        HandleAppend<TStream>(session, streamId, events?.ToList());
    }
}