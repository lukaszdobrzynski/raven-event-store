using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Raven.Client.Documents.Session;

namespace Raven.EventStore;

public partial class RavenEventStore
{
    private async Task<TStream> HandleCreateAsync<TStream>(IAsyncDocumentSession session, string streamId, List<Event> events)
        where TStream : DocumentStream, new()
    {
        CheckForNullEvents(events);
            
        AssignVersionToEvents(events, nextVersion: 1);
            
        var stream = CreateStream<TStream>(streamId, events);

        await session.StoreAsync(stream);

        var aggregate = BuildAggregate(stream);
            
        if (aggregate is not null)
        {
            await session.StoreAsync(aggregate);
            stream.AggregateId = aggregate.Id;
        }
            
        await AppendToGlobalLogAsync(session, stream.Id, stream.StreamKey, events);
        return stream;
    }
    
    private TStream HandleCreate<TStream>(IDocumentSession session, string streamId, List<Event> events)
        where TStream : DocumentStream, new()
    {
        CheckForNullEvents(events);
            
        AssignVersionToEvents(events, nextVersion: 1);
            
        var stream = CreateStream<TStream>(streamId, events);

        session.Store(stream);

        var aggregate = BuildAggregate(stream);
            
        if (aggregate is not null)
        {
            session.Store(aggregate);
            stream.AggregateId = aggregate.Id;
        }
            
        AppendToGlobalLog(session, stream.Id, stream.StreamKey, events);
        return stream;
    }

    private static TStream CreateStream<TStream>(string streamId, List<Event> events) where TStream : DocumentStream, new()
    {
        var stream = new TStream
        {
            Id = streamId,
            StreamKey = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            Events = events
        };
        
        return stream;
    }
}