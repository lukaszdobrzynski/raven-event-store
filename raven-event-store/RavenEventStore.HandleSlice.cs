using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Raven.Client.Documents.Session;
using Raven.EventStore.Exceptions;

namespace Raven.EventStore;

public partial class RavenEventStore
{
    private async Task<TStream> HandleSliceAsync<TStream>(IAsyncDocumentSession session, string sourceStreamId, string newStreamId, List<Event> events) where TStream : DocumentStream, new()
    {
        CheckForNullOrEmptyEvents(events);
            
        var sourceStream = await session.Include<TStream>(x => x.AggregateId)
            .LoadAsync(sourceStreamId);

        if (sourceStream is null)
        {
            throw new NonExistentStreamException(sourceStreamId);
        }
            
        AssignVersionToEvents(events, sourceStream.Position + 1);
            
        var aggregate = await session.LoadAsync<Aggregate>(sourceStream.AggregateId);
        sourceStream.Archive = aggregate;
            
        var newStream = new TStream
        {
            Id = newStreamId,
            CreatedAt = DateTime.UtcNow,
            Events = events,
            StreamKey = sourceStream.StreamKey,
            AggregateId = sourceStream.AggregateId,
            Seed = aggregate
        };

        var newAggregate = BuildAggregate(newStream);

        if (aggregate is not null)
        {
            var changeVector = session.Advanced.GetChangeVectorFor(aggregate);
            session.Advanced.Evict(aggregate);
            await session.StoreAsync(newAggregate, changeVector, aggregate.Id);
        }
            
        await session.StoreAsync(sourceStream);
        await session.StoreAsync(newStream);
            
        await AppendToGlobalLogAsync(session, newStream.Id, newStream.StreamKey, events);
        return newStream;
    }
    
    private TStream HandleSlice<TStream>(IDocumentSession session, string sourceStreamId, string newStreamId, List<Event> events) where TStream : DocumentStream, new()
    {
        CheckForNullOrEmptyEvents(events);
            
        var sourceStream = session.Include<TStream>(x => x.AggregateId)
            .Load(sourceStreamId);

        if (sourceStream is null)
        {
            throw new NonExistentStreamException(sourceStreamId);
        }
            
        AssignVersionToEvents(events, sourceStream.Position + 1);
            
        var aggregate = session.Load<Aggregate>(sourceStream.AggregateId);
        sourceStream.Archive = aggregate;
            
        var newStream = new TStream
        {
            Id = newStreamId,
            CreatedAt = DateTime.UtcNow,
            Events = events,
            StreamKey = sourceStream.StreamKey,
            AggregateId = sourceStream.AggregateId,
            Seed = aggregate
        };

        var newAggregate = BuildAggregate(newStream);

        if (aggregate is not null)
        {
            var changeVector = session.Advanced.GetChangeVectorFor(aggregate);
            session.Advanced.Evict(aggregate);
            session.Store(newAggregate, changeVector, aggregate.Id);
        }
            
        session.Store(sourceStream);
        session.Store(newStream);
            
        AppendToGlobalLog(session, newStream.Id, newStream.StreamKey, events);
        return newStream;
    }
}