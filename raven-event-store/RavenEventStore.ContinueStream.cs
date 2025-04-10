using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Raven.EventStore.Exceptions;

namespace Raven.EventStore;

public partial class RavenEventStore
{
    public Task<TStream> ContinueStreamAsync<TStream>(string originStreamId, string derivedStreamId, IEnumerable<Event> events) where TStream : DocumentStream, new()
    {
        return ContinueStreamAsync<TStream>(originStreamId, derivedStreamId, events?.ToList(), useOptimisticConcurrency:false);
    }
    
    public Task<TStream> ContinueStreamAsyncOptimistically<TStream>(string originStreamId, string derivedStreamId, IEnumerable<Event> events) where TStream : DocumentStream, new()
    {
        return ContinueStreamAsync<TStream>(originStreamId, derivedStreamId, events?.ToList(), useOptimisticConcurrency:true);
    }
    
    private async Task<TStream> ContinueStreamAsync<TStream>(string originStreamId,
        string derivedStreamId, List<Event> events, bool useOptimisticConcurrency) where TStream : DocumentStream, new()
    {
        CheckForNullOrEmptyEvents(events);

        using (var session = DocumentStore.OpenAsyncSession())
        {
            session.Advanced.UseOptimisticConcurrency = useOptimisticConcurrency;
            
            var originStream = await session.LoadAsync<TStream>(originStreamId)
                               ?? throw new NonExistentStreamException(originStreamId);

            AssignVersionToEvents(events, originStream.Position + 1);
            var originStreamSnapshot = TakeSnapshot(originStream);
            originStream.ArchivedSnapshot = originStreamSnapshot;
            
            var derivedStream = new TStream
            {
                Id = derivedStreamId,
                CreatedAt = DateTime.UtcNow,
                Events = events,
                LogicalId = originStream.LogicalId,
                SeedSnapshot = originStreamSnapshot
            };

            var derivedStreamSnapshot = TakeSnapshot(derivedStream);

            await session.StoreAsync(originStream);
            await session.StoreAsync(derivedStream);
            await session.StoreAsync(derivedStreamSnapshot);
            
            await AppendToGlobalLogAsync(session, events, derivedStream.Id);

            await session.SaveChangesAsync();
            
            return derivedStream;
        }
    }
}