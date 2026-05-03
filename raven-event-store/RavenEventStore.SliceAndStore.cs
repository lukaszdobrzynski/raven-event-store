using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Raven.EventStore;

public partial class RavenEventStore
{
    public Task<TStream> SliceStreamAndStoreAsync<TStream>(string sourceStreamId, string newStreamId,
        params Event[] events) where TStream : DocumentStream, new()
    {
        return SliceStreamAndStoreAsync<TStream>(sourceStreamId, newStreamId, events?.ToList(), useOptimisticConcurrency: false, CancellationToken.None);
    }

    public Task<TStream> SliceStreamAndStoreAsyncOptimistically<TStream>(string sourceStreamId, string newStreamId,
        params Event[] events) where TStream : DocumentStream, new()
    {
        return SliceStreamAndStoreAsync<TStream>(sourceStreamId, newStreamId, events?.ToList(), useOptimisticConcurrency: true, CancellationToken.None);
    }

    public TStream SliceStreamAndStore<TStream>(string sourceStreamId, string newStreamId, params Event[] events)
        where TStream : DocumentStream, new()
    {
        return SliceStreamAndStore<TStream>(sourceStreamId, newStreamId, events?.ToList(), useOptimisticConcurrency:false);
    }
    
    public TStream SliceStreamAndStoreOptimistically<TStream>(string sourceStreamId, string newStreamId, params Event[] events)
        where TStream : DocumentStream, new()
    {
        return SliceStreamAndStore<TStream>(sourceStreamId, newStreamId, events?.ToList(), useOptimisticConcurrency:true);
    }
    
    public Task<TStream> SliceStreamAndStoreAsync<TStream>(string sourceStreamId, string newStreamId, IEnumerable<Event> events, CancellationToken cancellationToken = default) where TStream : DocumentStream, new()
    {
        return SliceStreamAndStoreAsync<TStream>(sourceStreamId, newStreamId, events?.ToList(), useOptimisticConcurrency: false, cancellationToken);
    }

    public Task<TStream> SliceStreamAndStoreAsyncOptimistically<TStream>(string sourceStreamId, string newStreamId, IEnumerable<Event> events, CancellationToken cancellationToken = default) where TStream : DocumentStream, new()
    {
        return SliceStreamAndStoreAsync<TStream>(sourceStreamId, newStreamId, events?.ToList(), useOptimisticConcurrency: true, cancellationToken);
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
    
    public Task<TStream> SliceStreamAndStoreAsync<TStream>(string sourceStreamId,
        params Event[] events) where TStream : DocumentStream, new()
    {
        return SliceStreamAndStoreAsync<TStream>(sourceStreamId, newStreamId: null, events?.ToList(), useOptimisticConcurrency: false, CancellationToken.None);
    }

    public Task<TStream> SliceStreamAndStoreAsyncOptimistically<TStream>(string sourceStreamId, params Event[] events) where TStream : DocumentStream, new()
    {
        return SliceStreamAndStoreAsync<TStream>(sourceStreamId, newStreamId: null, events?.ToList(), useOptimisticConcurrency: true, CancellationToken.None);
    }

    public TStream SliceStreamAndStore<TStream>(string sourceStreamId, params Event[] events)
        where TStream : DocumentStream, new()
    {
        return SliceStreamAndStore<TStream>(sourceStreamId, newStreamId: null, events?.ToList(), useOptimisticConcurrency:false);
    }
    
    public TStream SliceStreamAndStoreOptimistically<TStream>(string sourceStreamId, params Event[] events)
        where TStream : DocumentStream, new()
    {
        return SliceStreamAndStore<TStream>(sourceStreamId, newStreamId: null, events?.ToList(), useOptimisticConcurrency:true);
    }
    
    public Task<TStream> SliceStreamAndStoreAsync<TStream>(string sourceStreamId, IEnumerable<Event> events, CancellationToken cancellationToken = default) where TStream : DocumentStream, new()
    {
        return SliceStreamAndStoreAsync<TStream>(sourceStreamId, newStreamId: null, events?.ToList(), useOptimisticConcurrency: false, cancellationToken);
    }

    public Task<TStream> SliceStreamAndStoreAsyncOptimistically<TStream>(string sourceStreamId, IEnumerable<Event> events, CancellationToken cancellationToken = default) where TStream : DocumentStream, new()
    {
        return SliceStreamAndStoreAsync<TStream>(sourceStreamId, newStreamId: null, events?.ToList(), useOptimisticConcurrency: true, cancellationToken);
    }

    public TStream SliceStreamAndStore<TStream>(string sourceStreamId, IEnumerable<Event> events)
        where TStream : DocumentStream, new()
    {
        return SliceStreamAndStore<TStream>(sourceStreamId, newStreamId: null, events?.ToList(), useOptimisticConcurrency:false);
    }
    
    public TStream SliceStreamAndStoreOptimistically<TStream>(string sourceStreamId, IEnumerable<Event> events)
        where TStream : DocumentStream, new()
    {
        return SliceStreamAndStore<TStream>(sourceStreamId, newStreamId: null, events?.ToList(), useOptimisticConcurrency:true);
    }
    
    private async Task<TStream> SliceStreamAndStoreAsync<TStream>(string sourceStreamId,
        string newStreamId, List<Event> events, bool useOptimisticConcurrency, CancellationToken cancellationToken = default) where TStream : DocumentStream, new()
    {
        using (var session = OpenAsyncSession())
        {
            session.Advanced.UseOptimisticConcurrency = useOptimisticConcurrency;
            var newStream = await HandleSliceAsync<TStream>(session, sourceStreamId, newStreamId, events, cancellationToken);
            await session.SaveChangesAsync(cancellationToken);

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