using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Raven.EventStore;

public partial class RavenEventStore
{
    public Task<TStream> SliceStreamAndStoreAsync<TStream>(string sourceStreamId, string newStreamId, IEnumerable<Event> events) where TStream : DocumentStream, new()
    {
        return SliceStreamAndStoreAsync<TStream>(sourceStreamId, newStreamId, events?.ToList(), useOptimisticConcurrency:false);
    }
    
    public Task<TStream> SliceStreamAndStoreAsyncOptimistically<TStream>(string originStreamId, string derivedStreamId, IEnumerable<Event> events) where TStream : DocumentStream, new()
    {
        return SliceStreamAndStoreAsync<TStream>(originStreamId, derivedStreamId, events?.ToList(), useOptimisticConcurrency:true);
    }

    public TStream SliceStreamAndStore<TStream>(string sourceStreamId, string newStreamId, IEnumerable<Event> events)
        where TStream : DocumentStream, new()
    {
        return SliceStreamAndStore<TStream>(sourceStreamId, newStreamId, events?.ToList(), useOptimisticConcurrency:false);
    }
    
    public TStream SliceStreamAndStoreOptimistically<TStream>(string sourceStreamId, string newStreamId, IEnumerable<Event> events)
        where TStream : DocumentStream, new()
    {
        return SliceStreamAndStore<TStream>(sourceStreamId, newStreamId, events?.ToList(), useOptimisticConcurrency:true);
    }
    
    private async Task<TStream> SliceStreamAndStoreAsync<TStream>(string sourceStreamId,
        string newStreamId, List<Event> events, bool useOptimisticConcurrency) where TStream : DocumentStream, new()
    {
        using (var session = OpenAsyncSession())
        {
            session.Advanced.UseOptimisticConcurrency = useOptimisticConcurrency;
            var newStream = await HandleSliceAsync<TStream>(session, sourceStreamId, newStreamId, events);
            await session.SaveChangesAsync();
            
            return newStream;
        }
    }

    private TStream SliceStreamAndStore<TStream>(string sourceStreamId,
        string newStreamId, List<Event> events, bool useOptimisticConcurrency) where TStream : DocumentStream, new()
    {
        using (var session = OpenSession())
        {
            session.Advanced.UseOptimisticConcurrency = useOptimisticConcurrency;
            var newStream = HandleSlice<TStream>(session, sourceStreamId, newStreamId, events);
            session.SaveChanges();
            
            return newStream;
        }
    }
}