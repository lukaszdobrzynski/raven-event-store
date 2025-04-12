using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Raven.EventStore.Exceptions;

namespace Raven.EventStore;

public partial class RavenEventStore
{
    public Task<TStream> SliceStreamAsync<TStream>(string sourceStreamId, string newStreamId, IEnumerable<Event> events) where TStream : DocumentStream, new()
    {
        return SliceStreamAsync<TStream>(sourceStreamId, newStreamId, events?.ToList(), useOptimisticConcurrency:false);
    }
    
    public Task<TStream> SliceStreamAsyncOptimistically<TStream>(string originStreamId, string derivedStreamId, IEnumerable<Event> events) where TStream : DocumentStream, new()
    {
        return SliceStreamAsync<TStream>(originStreamId, derivedStreamId, events?.ToList(), useOptimisticConcurrency:true);
    }

    public TStream SliceStream<TStream>(string sourceStreamId, string newStreamId, IEnumerable<Event> events)
        where TStream : DocumentStream, new()
    {
        return SliceStream<TStream>(sourceStreamId, newStreamId, events?.ToList(), useOptimisticConcurrency:false);
    }
    
    public TStream SliceStreamOptimistically<TStream>(string sourceStreamId, string newStreamId, IEnumerable<Event> events)
        where TStream : DocumentStream, new()
    {
        return SliceStream<TStream>(sourceStreamId, newStreamId, events?.ToList(), useOptimisticConcurrency:true);
    }
    
    private async Task<TStream> SliceStreamAsync<TStream>(string sourceStreamId,
        string newStreamId, List<Event> events, bool useOptimisticConcurrency) where TStream : DocumentStream, new()
    {
        CheckForNullOrEmptyEvents(events);

        using (var session = DocumentStore.OpenAsyncSession())
        {
            session.Advanced.UseOptimisticConcurrency = useOptimisticConcurrency;

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
                aggregate.Id = aggregate.Id;
                await session.StoreAsync(newAggregate, changeVector, newAggregate.Id);
            }
            
            await session.StoreAsync(sourceStream);
            await session.StoreAsync(newStream);
            
            await AppendToGlobalLogAsync(session, events, newStream.Id);

            await session.SaveChangesAsync();
            
            return newStream;
        }
    }
    
    private TStream SliceStream<TStream>(string sourceStreamId,
        string newStreamId, List<Event> events, bool useOptimisticConcurrency) where TStream : DocumentStream, new()
    {
        CheckForNullOrEmptyEvents(events);

        using (var session = DocumentStore.OpenSession())
        {
            session.Advanced.UseOptimisticConcurrency = useOptimisticConcurrency;

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
                newAggregate.Id = aggregate.Id;
                session.Store(newAggregate, changeVector, newAggregate.Id);
            }
            
            session.Store(sourceStream);
            session.Store(newStream);
            
            AppendToGlobalLog(session, events, newStream.Id);

            session.SaveChanges();
            
            return newStream;
        }
    }
}