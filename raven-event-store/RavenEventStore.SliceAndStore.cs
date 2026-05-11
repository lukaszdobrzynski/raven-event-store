using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Raven.EventStore;

public partial class RavenEventStore
{
    public Task<TStream> SliceStreamAndStoreAsync<TStream>(Guid streamKey, string newStreamId,
        params Event[] events) where TStream : DocumentStream, new()
    {
        return SliceStreamAndStoreAsync<TStream>(streamKey, newStreamId, events?.ToList(), useOptimisticConcurrency: false, CancellationToken.None);
    }

    public Task<TStream> SliceStreamAndStoreAsyncOptimistically<TStream>(Guid streamKey, string newStreamId,
        params Event[] events) where TStream : DocumentStream, new()
    {
        return SliceStreamAndStoreAsync<TStream>(streamKey, newStreamId, events?.ToList(), useOptimisticConcurrency: true, CancellationToken.None);
    }

    public TStream SliceStreamAndStore<TStream>(Guid streamKey, string newStreamId, params Event[] events)
        where TStream : DocumentStream, new()
    {
        return SliceStreamAndStore<TStream>(streamKey, newStreamId, events?.ToList(), useOptimisticConcurrency: false);
    }

    public TStream SliceStreamAndStoreOptimistically<TStream>(Guid streamKey, string newStreamId, params Event[] events)
        where TStream : DocumentStream, new()
    {
        return SliceStreamAndStore<TStream>(streamKey, newStreamId, events?.ToList(), useOptimisticConcurrency: true);
    }

    public Task<TStream> SliceStreamAndStoreAsync<TStream>(Guid streamKey, string newStreamId, IEnumerable<Event> events, CancellationToken cancellationToken = default) where TStream : DocumentStream, new()
    {
        return SliceStreamAndStoreAsync<TStream>(streamKey, newStreamId, events?.ToList(), useOptimisticConcurrency: false, cancellationToken);
    }

    public Task<TStream> SliceStreamAndStoreAsyncOptimistically<TStream>(Guid streamKey, string newStreamId, IEnumerable<Event> events, CancellationToken cancellationToken = default) where TStream : DocumentStream, new()
    {
        return SliceStreamAndStoreAsync<TStream>(streamKey, newStreamId, events?.ToList(), useOptimisticConcurrency: true, cancellationToken);
    }

    public TStream SliceStreamAndStore<TStream>(Guid streamKey, string newStreamId, IEnumerable<Event> events)
        where TStream : DocumentStream, new()
    {
        return SliceStreamAndStore<TStream>(streamKey, newStreamId, events?.ToList(), useOptimisticConcurrency: false);
    }

    public TStream SliceStreamAndStoreOptimistically<TStream>(Guid streamKey, string newStreamId, IEnumerable<Event> events)
        where TStream : DocumentStream, new()
    {
        return SliceStreamAndStore<TStream>(streamKey, newStreamId, events?.ToList(), useOptimisticConcurrency: true);
    }

    public Task<TStream> SliceStreamAndStoreAsync<TStream>(Guid streamKey,
        params Event[] events) where TStream : DocumentStream, new()
    {
        return SliceStreamAndStoreAsync<TStream>(streamKey, newStreamId: null, events?.ToList(), useOptimisticConcurrency: false, CancellationToken.None);
    }

    public Task<TStream> SliceStreamAndStoreAsyncOptimistically<TStream>(Guid streamKey, params Event[] events) where TStream : DocumentStream, new()
    {
        return SliceStreamAndStoreAsync<TStream>(streamKey, newStreamId: null, events?.ToList(), useOptimisticConcurrency: true, CancellationToken.None);
    }

    public TStream SliceStreamAndStore<TStream>(Guid streamKey, params Event[] events)
        where TStream : DocumentStream, new()
    {
        return SliceStreamAndStore<TStream>(streamKey, newStreamId: null, events?.ToList(), useOptimisticConcurrency: false);
    }

    public TStream SliceStreamAndStoreOptimistically<TStream>(Guid streamKey, params Event[] events)
        where TStream : DocumentStream, new()
    {
        return SliceStreamAndStore<TStream>(streamKey, newStreamId: null, events?.ToList(), useOptimisticConcurrency: true);
    }

    public Task<TStream> SliceStreamAndStoreAsync<TStream>(Guid streamKey, IEnumerable<Event> events, CancellationToken cancellationToken = default) where TStream : DocumentStream, new()
    {
        return SliceStreamAndStoreAsync<TStream>(streamKey, newStreamId: null, events?.ToList(), useOptimisticConcurrency: false, cancellationToken);
    }

    public Task<TStream> SliceStreamAndStoreAsyncOptimistically<TStream>(Guid streamKey, IEnumerable<Event> events, CancellationToken cancellationToken = default) where TStream : DocumentStream, new()
    {
        return SliceStreamAndStoreAsync<TStream>(streamKey, newStreamId: null, events?.ToList(), useOptimisticConcurrency: true, cancellationToken);
    }

    public TStream SliceStreamAndStore<TStream>(Guid streamKey, IEnumerable<Event> events)
        where TStream : DocumentStream, new()
    {
        return SliceStreamAndStore<TStream>(streamKey, newStreamId: null, events?.ToList(), useOptimisticConcurrency: false);
    }

    public TStream SliceStreamAndStoreOptimistically<TStream>(Guid streamKey, IEnumerable<Event> events)
        where TStream : DocumentStream, new()
    {
        return SliceStreamAndStore<TStream>(streamKey, newStreamId: null, events?.ToList(), useOptimisticConcurrency: true);
    }

    private async Task<TStream> SliceStreamAndStoreAsync<TStream>(Guid streamKey,
        string newStreamId, List<Event> events, bool useOptimisticConcurrency, CancellationToken cancellationToken = default) where TStream : DocumentStream, new()
    {
        using (var session = OpenAsyncSession())
        {
            session.Advanced.UseOptimisticConcurrency = useOptimisticConcurrency;
            var newStream = await HandleSliceAsync<TStream>(session, streamKey, newStreamId, events, cancellationToken);
            await session.SaveChangesAsync(cancellationToken);

            return newStream;
        }
    }

    private TStream SliceStreamAndStore<TStream>(Guid streamKey,
        string newStreamId, List<Event> events, bool useOptimisticConcurrency) where TStream : DocumentStream, new()
    {
        using (var session = OpenSession())
        {
            session.Advanced.UseOptimisticConcurrency = useOptimisticConcurrency;
            var newStream = HandleSlice<TStream>(session, streamKey, newStreamId, events);
            session.SaveChanges();

            return newStream;
        }
    }
}
