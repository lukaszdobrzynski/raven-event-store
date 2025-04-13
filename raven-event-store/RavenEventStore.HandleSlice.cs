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
        
        var newStreamSlice = CreateNewStreamSlice<TStream>(newStreamId, sourceStream.StreamKey, sourceStream.AggregateId, aggregate, events);
        var newAggregate = BuildAggregate(newStreamSlice);

        if (aggregate is not null)
        {
            var changeVector = session.Advanced.GetChangeVectorFor(aggregate);
            session.Advanced.Evict(aggregate);
            await session.StoreAsync(newAggregate, changeVector, aggregate.Id);
        }
            
        await session.StoreAsync(sourceStream);
        await session.StoreAsync(newStreamSlice);
            
        await AppendToGlobalLogAsync(session, newStreamSlice.Id, newStreamSlice.StreamKey, events);
        return newStreamSlice;
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
            
        var newStreamSlice = CreateNewStreamSlice<TStream>(newStreamId, sourceStream.StreamKey, sourceStream.AggregateId, aggregate, events);
        var newAggregate = BuildAggregate(newStreamSlice);

        if (aggregate is not null)
        {
            var changeVector = session.Advanced.GetChangeVectorFor(aggregate);
            session.Advanced.Evict(aggregate);
            session.Store(newAggregate, changeVector, aggregate.Id);
        }
            
        session.Store(sourceStream);
        session.Store(newStreamSlice);
            
        AppendToGlobalLog(session, newStreamSlice.Id, newStreamSlice.StreamKey, events);
        return newStreamSlice;
    }

    private static TStream CreateNewStreamSlice<TStream>(string newStreamId, Guid streamKey, string aggregateId, Aggregate seed, List<Event> events) where TStream : DocumentStream, new()
    {
        var newStream = new TStream
        {
            Id = newStreamId,
            CreatedAt = DateTime.UtcNow,
            Events = events,
            StreamKey = streamKey,
            AggregateId = aggregateId,
            Seed = seed
        };

        return newStream;
    }
}