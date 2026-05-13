using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Raven.Client.Documents.Session;

namespace Raven.EventStore;

public partial class RavenEventStore
{
    public async Task AppendAsync(IAsyncDocumentSession session, Guid streamKey, IEnumerable<Event> events, CancellationToken cancellationToken = default)
    {
        await HandleAppendAsync(session, streamKey, events?.ToList(), cancellationToken);
    }

    public void Append(IDocumentSession session, Guid streamKey, IEnumerable<Event> events)
    {
        HandleAppend(session, streamKey, events?.ToList());
    }

    public async Task AppendAsync(IAsyncDocumentSession session, Guid streamKey, params Event[] events)
    {
        await HandleAppendAsync(session, streamKey, events?.ToList(), CancellationToken.None);
    }

    public void Append(IDocumentSession session, Guid streamKey, params Event[] events)
    {
        HandleAppend(session, streamKey, events?.ToList());
    }
}
