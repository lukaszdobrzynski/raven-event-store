using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Raven.Client.Documents.Session;

namespace Raven.EventStore;

public partial class RavenEventStore
{
    private void HandleAppend<TStream>(IDocumentSession session, string streamId, List<Event> eventList)
        where TStream : DocumentStream
    {
        CheckForNullOrEmptyEvents(eventList);

        var stream = session.Load<TStream>(streamId);

        CheckForNonExistentStream(stream, streamId);
        AssignVersionToEvents(eventList, nextVersion: stream.Position + 1);
        
        stream.Events.AddRange(eventList);
        stream.UpdatedAt = DateTime.UtcNow;

        var aggregate = BuildAggregate(stream);

        if (aggregate is not null)
        {
            aggregate.Id = stream.AggregateId;
            session.Store(aggregate);
        }
         
        AppendToGlobalLog(session, eventList, streamId);
    }
    
    private async Task HandleAppendAsync<TStream>(IAsyncDocumentSession session, string streamId, List<Event> eventList)
        where TStream : DocumentStream
    {
        CheckForNullOrEmptyEvents(eventList);

        var stream = await session.LoadAsync<TStream>(streamId);

        CheckForNonExistentStream(stream, streamId);
        AssignVersionToEvents(eventList, nextVersion: stream.Position + 1);
        
        stream.Events.AddRange(eventList);
        stream.UpdatedAt = DateTime.UtcNow;

        var aggregate = BuildAggregate(stream);

        if (aggregate is not null)
        {
            aggregate.Id = stream.AggregateId;
            await session.StoreAsync(aggregate);
        }
         
        await AppendToGlobalLogAsync(session, eventList, streamId);
    }
}