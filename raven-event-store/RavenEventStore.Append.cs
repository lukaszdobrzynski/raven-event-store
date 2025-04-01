using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Raven.EventStore;

public partial class RavenEventStore
{
    public async Task AppendAsync<TStream>(string streamId, List<Event> events) where TStream : DocumentStream
    {
        await AppendAsync<TStream>(streamId, events, useOptimisticConcurrency: false);
    }
    
    public void Append<TStream>(string streamId, List<Event> events) where TStream : DocumentStream
    {
        Append<TStream>(streamId, events, useOptimisticConcurrency: false);
    }
    
    public async Task AppendAsyncOptimistically<TStream>(string streamId, List<Event> events) where TStream : DocumentStream
    {
        await AppendAsync<TStream>(streamId, events, useOptimisticConcurrency: true);
    }
    
    public void AppendOptimistically<TStream>(string streamId, List<Event> events) where TStream : DocumentStream
    {
        Append<TStream>(streamId, events, useOptimisticConcurrency: true);
    }
    
    public async Task AppendAsync<TStream>(string streamId, params Event[] events) where TStream : DocumentStream
    {
        await AppendAsync<TStream>(streamId, events?.ToList(), useOptimisticConcurrency: false);
    }
    
    public void Append<TStream>(string streamId, params Event[] events) where TStream : DocumentStream
    {
        Append<TStream>(streamId, events?.ToList(), useOptimisticConcurrency: false);
    }
    
    public async Task AppendAsyncOptimistically<TStream>(string streamId, params Event[] events) where TStream : DocumentStream
    {
        await AppendAsync<TStream>(streamId, events?.ToList(), useOptimisticConcurrency: true);
    }
    
    public void AppendOptimistically<TStream>(string streamId, params Event[] events) where TStream : DocumentStream
    {
        Append<TStream>(streamId, events?.ToList(), useOptimisticConcurrency: true);
    }
    
    private async Task AppendAsync<TStream>(string streamId, List<Event> events, bool useOptimisticConcurrency) where TStream : DocumentStream
    {
        CheckForNullOrEmptyEvents(events);

        using (var session = DocumentStore.OpenAsyncSession())
        {
            session.Advanced.UseOptimisticConcurrency = useOptimisticConcurrency;
            var stream = await session.LoadAsync<TStream>(streamId);

            CheckForNonExistentStream(stream, streamId);
            AssignVersionToEvents(events, nextVersion: stream.Position + 1);
            
            stream.Events.AddRange(events);
            stream.UpdatedAt = DateTime.UtcNow;
            
            await RunProjectionAndStoreAsync(stream, session);
            await AppendToGlobalLogAsync(session, events, stream.Id);
            
            await session.SaveChangesAsync();
        }
    }
    
    private void Append<TStream>(string streamId, List<Event> events, bool useOptimisticConcurrency) where TStream : DocumentStream
    {
        CheckForNullOrEmptyEvents(events);

        using (var session = DocumentStore.OpenSession())
        {
            session.Advanced.UseOptimisticConcurrency = useOptimisticConcurrency;
            var stream = session.Load<TStream>(streamId);

            CheckForNonExistentStream(stream, streamId);
            AssignVersionToEvents(events, nextVersion: stream.Position + 1);
            
            stream.Events.AddRange(events);
            stream.UpdatedAt = DateTime.UtcNow;
            
            RunProjectionAndStore(stream, session);
            AppendToGlobalLog(session, events, streamId);
            
            session.SaveChanges();
        }
    }
}