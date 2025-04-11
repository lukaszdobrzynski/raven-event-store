using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Raven.EventStore;

public partial class RavenEventStore
{
    public Task<TStream> CreateStreamAsync<TStream>(IEnumerable<Event> events) where TStream : DocumentStream, new()
    {
        return CreateAndPersistStreamAsync<TStream>(null, events?.ToList());
    }
    
    public TStream CreateStream<TStream>(IEnumerable<Event> events) where TStream : DocumentStream, new()
    {
        return CreateAndPersistStream<TStream>(null, events?.ToList());
    }

    public Task<TStream> CreateStreamAsync<TStream>(string streamId, IEnumerable<Event> events)
        where TStream : DocumentStream, new()
    {
        return CreateAndPersistStreamAsync<TStream>(streamId, events?.ToList());
    }
    
    public TStream CreateStream<TStream>(string streamId, IEnumerable<Event> events)
        where TStream : DocumentStream, new()
    {
        return CreateAndPersistStream<TStream>(streamId, events?.ToList());
    }
    
    public Task<TStream> CreateStreamAsync<TStream>(params Event[] events) where TStream : DocumentStream, new()
    {
        return CreateAndPersistStreamAsync<TStream>(null, events?.ToList());
    }
    
    public TStream CreateStream<TStream>(params Event[] events) where TStream : DocumentStream, new()
    {
        return CreateAndPersistStream<TStream>(null, events?.ToList());
    }
    
    public Task<TStream> CreateStreamAsync<TStream>(string streamId, params Event[] events)
        where TStream : DocumentStream, new()
    {
        return CreateAndPersistStreamAsync<TStream>(streamId, events?.ToList());
    }
    
    public TStream CreateStream<TStream>(string streamId, params Event[] events)
        where TStream : DocumentStream, new()
    {
        return CreateAndPersistStream<TStream>(streamId, events?.ToList());
    }
    
    private async Task<TStream> CreateAndPersistStreamAsync<TStream>(string streamId, List<Event> events) where TStream : DocumentStream, new()
    {
        CheckForNullOrEmptyEvents(events);
        
        using (var session = DocumentStore.OpenAsyncSession())
        {
            AssignVersionToEvents(events, nextVersion: 1);
            
            var stream = new TStream
            {
                Id = streamId,
                LogicalId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                Events = events
            };

            await session.StoreAsync(stream);

            var snapshot = TakeSnapshot(stream);

            if (snapshot is not null)
            {
                await session.StoreAsync(snapshot);
                stream.SnapshotId = snapshot.Id;
            }
            
            await AppendToGlobalLogAsync(session, events, stream.Id);
            
            await session.SaveChangesAsync();

            return stream;
        }
    }
    
    private TStream CreateAndPersistStream<TStream>(string streamId, List<Event> events) where TStream : DocumentStream, new()
    {
        CheckForNullOrEmptyEvents(events);
        
        using (var session = DocumentStore.OpenSession())
        {
            AssignVersionToEvents(events, nextVersion: 1);
            
            var stream = new TStream
            {
                Id = streamId,
                LogicalId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                Events = events
            };

            session.Store(stream);

            var snapshot = TakeSnapshot(stream);
            
            if (snapshot is not null)
            {
                session.Store(snapshot);
            }
            
            AppendToGlobalLog(session, events, stream.Id);
            
            session.SaveChanges();

            return stream;
        }
    }
}