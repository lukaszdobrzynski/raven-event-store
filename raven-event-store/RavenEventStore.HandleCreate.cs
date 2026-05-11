using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Raven.Client.Documents.Session;

namespace Raven.EventStore;

public partial class RavenEventStore
{
    private async Task<TStream> HandleCreateAsync<TStream>(IAsyncDocumentSession session, string streamId, List<Event> events, CancellationToken cancellationToken = default)
        where TStream : DocumentStream, new()
    {
        CheckForNullOrEmptyEvents(events);

        AssignVersionToEvents(events, nextVersion: 1);

        var stream = CreateStream<TStream>(streamId, events);

        await session.StoreAsync(stream, cancellationToken);

        var aggregate = BuildAggregate(stream);

        if (aggregate is not null)
        {
            await session.StoreAsync(aggregate, cancellationToken);
            stream.AggregateId = aggregate.Id;
        }

        var header = StreamHeader.Create(
            stream.StreamKey,
            stream.Id,
            stream.AggregateId,
            headPosition: stream.Position,
            headFirstVersion: stream.Events[0].Version,
            headFirstTimestamp: stream.Events[0].Timestamp);
        
        await session.StoreAsync(header, cancellationToken);

        await AppendToGlobalLogAsync(session, stream.Id, stream.StreamKey, events, cancellationToken);
        return stream;
    }
    
    private TStream HandleCreate<TStream>(IDocumentSession session, string streamId, List<Event> events)
        where TStream : DocumentStream, new()
    {
        CheckForNullOrEmptyEvents(events);
            
        AssignVersionToEvents(events, nextVersion: 1);
            
        var stream = CreateStream<TStream>(streamId, events);

        session.Store(stream);

        var aggregate = BuildAggregate(stream);

        if (aggregate is not null)
        {
            session.Store(aggregate);
            stream.AggregateId = aggregate.Id;
        }

        var header = StreamHeader.Create(
            stream.StreamKey,
            stream.Id,
            stream.AggregateId,
            headPosition: stream.Position,
            headFirstVersion: stream.Events[0].Version,
            headFirstTimestamp: stream.Events[0].Timestamp);
        session.Store(header);

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