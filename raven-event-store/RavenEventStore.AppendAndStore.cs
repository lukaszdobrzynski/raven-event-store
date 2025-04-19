using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Raven.EventStore;

public partial class RavenEventStore
{
    public async Task AppendAndStoreAsync<TStream>(string streamId, IEnumerable<Event> events) where TStream : DocumentStream
    {
        await AppendAndStoreAsync<TStream>(streamId, events?.ToList(), useOptimisticConcurrency: false);
    }
    
    public void AppendAndStore<TStream>(string streamId, IEnumerable<Event> events) where TStream : DocumentStream
    {
        AppendAndStore<TStream>(streamId, events?.ToList(), useOptimisticConcurrency: false);
    }
    
    public async Task AppendAndStoreAsyncOptimistically<TStream>(string streamId, IEnumerable<Event> events) where TStream : DocumentStream
    {
        await AppendAndStoreAsync<TStream>(streamId, events?.ToList(), useOptimisticConcurrency: true);
    }
    
    public void AppendAndStoreOptimistically<TStream>(string streamId, IEnumerable<Event> events) where TStream : DocumentStream
    {
        AppendAndStore<TStream>(streamId, events?.ToList(), useOptimisticConcurrency: true);
    }
    
    public async Task AppendAndStoreAsync<TStream>(string streamId, params Event[] events) where TStream : DocumentStream
    {
        await AppendAndStoreAsync<TStream>(streamId, events?.ToList(), useOptimisticConcurrency: false);
    }
    
    public void AppendAndStore<TStream>(string streamId, params Event[] events) where TStream : DocumentStream
    {
        AppendAndStore<TStream>(streamId, events?.ToList(), useOptimisticConcurrency: false);
    }
    
    public async Task AppendAndStoreAsyncOptimistically<TStream>(string streamId, params Event[] events) where TStream : DocumentStream
    {
        await AppendAndStoreAsync<TStream>(streamId, events?.ToList(), useOptimisticConcurrency: true);
    }
    
    public void AppendAndStoreOptimistically<TStream>(string streamId, params Event[] events) where TStream : DocumentStream
    {
        AppendAndStore<TStream>(streamId, events?.ToList(), useOptimisticConcurrency: true);
    }
    
    private async Task AppendAndStoreAsync<TStream>(string streamId, List<Event> events, bool useOptimisticConcurrency) where TStream : DocumentStream
    {
        CheckForNullEvents(events);

        using (var session = OpenAsyncSession())
        {
            session.Advanced.UseOptimisticConcurrency = useOptimisticConcurrency;
            await HandleAppendAsync<TStream>(session, streamId, events);
            await session.SaveChangesAsync();
        }
    }
    
    private void AppendAndStore<TStream>(string streamId, List<Event> events, bool useOptimisticConcurrency) where TStream : DocumentStream
    {
        CheckForNullEvents(events);

        using (var session = OpenSession())
        {
            session.Advanced.UseOptimisticConcurrency = useOptimisticConcurrency;
            HandleAppend<TStream>(session, streamId, events);
            session.SaveChanges();
        }
    }
}