using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Raven.Client.Documents.Session;

namespace Raven.EventStore;

public partial class RavenEventStore
{
    private void HandleAppend<TStream>(IDocumentSession session, string streamId, List<Event> events)
        where TStream : DocumentStream
    {
        CheckForNullEvents(events);

        var stream = session.Load<TStream>(streamId);

        CheckForNonExistentStream(stream, streamId);
        CheckForAttemptToAppendToNonHead(stream);
        AssignVersionToEvents(events, nextVersion: stream.Position + 1);
        
        AddEventsToStream(stream, events);

        var aggregate = BuildAggregate(stream);

        if (aggregate is not null)
        {
            aggregate.Id = stream.AggregateId;
            session.Store(aggregate);
        }
         
        AppendToGlobalLog(session, streamId, stream.StreamKey, events);
    }
    
    private async Task HandleAppendAsync<TStream>(IAsyncDocumentSession session, string streamId, List<Event> events)
        where TStream : DocumentStream
    {
        CheckForNullEvents(events);

        var stream = await session.LoadAsync<TStream>(streamId);

        CheckForNonExistentStream(stream, streamId);
        CheckForAttemptToAppendToNonHead(stream);
        AssignVersionToEvents(events, nextVersion: stream.Position + 1);
        
        AddEventsToStream(stream, events);

        var aggregate = BuildAggregate(stream);

        if (aggregate is not null)
        {
            aggregate.Id = stream.AggregateId;
            await session.StoreAsync(aggregate);
        }
         
        await AppendToGlobalLogAsync(session, streamId, stream.StreamKey, events);
    }

    private static void AddEventsToStream<TStream>(TStream stream, List<Event> events) where TStream : DocumentStream
    {
        stream.Events.AddRange(events);
        stream.UpdatedAt = DateTime.UtcNow;
    }
}