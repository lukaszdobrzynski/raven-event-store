using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Raven.EventStore;

public partial class RavenEventStore
{
    public Task<TStream> CreateStreamAndStoreAsync<TStream>(IEnumerable<Event> events, CancellationToken cancellationToken = default) where TStream : DocumentStream, new()
    {
        return CreateStreamAndStoreAsync<TStream>(null, events?.ToList(), cancellationToken);
    }
    
    public TStream CreateStreamAndStore<TStream>(IEnumerable<Event> events) where TStream : DocumentStream, new()
    {
        return CreateStreamAndStore<TStream>(null, events?.ToList());
    }

    public Task<TStream> CreateStreamAndStoreAsync<TStream>(string streamId, IEnumerable<Event> events, CancellationToken cancellationToken = default)
        where TStream : DocumentStream, new()
    {
        return CreateStreamAndStoreAsync<TStream>(streamId, events?.ToList(), cancellationToken);
    }
    
    public TStream CreateStreamAndStore<TStream>(string streamId, IEnumerable<Event> events)
        where TStream : DocumentStream, new()
    {
        return CreateStreamAndStore<TStream>(streamId, events?.ToList());
    }
    
    public Task<TStream> CreateStreamAndStoreAsync<TStream>(params Event[] events) where TStream : DocumentStream, new()
    {
        return CreateStreamAndStoreAsync<TStream>(null, events?.ToList(), CancellationToken.None);
    }
    
    public TStream CreateStreamAndStore<TStream>(params Event[] events) where TStream : DocumentStream, new()
    {
        return CreateStreamAndStore<TStream>(null, events?.ToList());
    }
    
    public Task<TStream> CreateStreamAndStoreAsync<TStream>(string streamId, params Event[] events)
        where TStream : DocumentStream, new()
    {
        return CreateStreamAndStoreAsync<TStream>(streamId, events?.ToList(), CancellationToken.None);
    }
    
    public TStream CreateStreamAndStore<TStream>(string streamId, params Event[] events)
        where TStream : DocumentStream, new()
    {
        return CreateStreamAndStore<TStream>(streamId, events?.ToList());
    }
    
    private async Task<TStream> CreateStreamAndStoreAsync<TStream>(string streamId, List<Event> events, CancellationToken cancellationToken = default) where TStream : DocumentStream, new()
    {
        using (var session = OpenAsyncSession())
        {
            var stream = await HandleCreateAsync<TStream>(session, streamId, events, cancellationToken);
            await session.SaveChangesAsync(cancellationToken);
            return stream;
        }
    }
    
    private TStream CreateStreamAndStore<TStream>(string streamId, List<Event> events) where TStream : DocumentStream, new()
    {
        using (var session = OpenSession())
        {
            var stream = HandleCreate<TStream>(session, streamId, events);
            session.SaveChanges();
            return stream;
        }
    }
}