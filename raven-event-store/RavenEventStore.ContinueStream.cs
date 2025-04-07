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
        return ContinueStreamAsync<TStream>(originStreamId, derivedStreamId, events?.ToList());
    }
    
    private async Task<TStream> ContinueStreamAsync<TStream>(string originStreamId,
        string derivedStreamId, List<Event> events) where TStream : DocumentStream, new()
    {
        CheckForNullOrEmptyEvents(events);

        using (var session = DocumentStore.OpenAsyncSession())
        {
            var originStream = await session.LoadAsync<TStream>(originStreamId);

            if (originStream is null)
                throw new NonExistentStreamException(originStreamId);
            
            AssignVersionToEvents(events, originStream.Position + 1);
            var originStreamSnapshot = TakeSnapshot(originStream);

            originStream.ArchiveSnapshot = originStreamSnapshot;
            
            var derivedStream = new TStream
            {
                Id = derivedStreamId,
                CreatedAt = DateTime.UtcNow,
                Events = events,
                SeedSnapshot = originStreamSnapshot
            };

            originStream.ArchiveSnapshot = originStreamSnapshot;

            var derivedStreamSnapshot = TakeSnapshot(derivedStream);
            derivedStreamSnapshot.Id = originStreamSnapshot.Id;

            await session.StoreAsync(originStream);
            await session.StoreAsync(derivedStream);
            await session.StoreAsync(derivedStreamSnapshot);
            
            await AppendToGlobalLogAsync(session, events, derivedStream.Id);

            await session.SaveChangesAsync();
            
            return derivedStream;
        }
    }
}