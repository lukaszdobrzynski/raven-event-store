using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Raven.Client.Documents.Session;

namespace Raven.EventStore;

public partial class RavenEventStore
{
    private async Task AppendToGlobalLogAsync(IAsyncDocumentSession session, string streamId, Guid streamKey, List<Event> events)
    {
        if (UseGlobalStreamLogging)
        {
            foreach (var @event in events)
            {
                var log = CreateGlobalEventLog(streamId, streamKey, @event);
                await session.StoreAsync(log);
            }    
        }
    }
    
    private void AppendToGlobalLog(IDocumentSession session, string streamId, Guid streamKey, List<Event> events)
    {
        if (UseGlobalStreamLogging)
        {
            foreach (var @event in events)
            {
                var log = CreateGlobalEventLog(streamId, streamKey, @event);
                session.Store(log);
            }    
        }
    }

    private GlobalEventLog CreateGlobalEventLog(string streamId, Guid streamKey, Event @event)
    {
        var sequence = GetSequence();
        return GlobalEventLog.From(sequence, streamId, streamKey, @event);
    }

    private static string GetSequence() => GlobalEventLogSequentialIdGenerator.CreateId().ToString();
}