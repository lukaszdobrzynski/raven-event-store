using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Raven.EventStore;

public partial class RavenEventStore
{
    public Task<TStream> CreateStreamAndStoreAsync<TStream>(IEnumerable<Event> events) where TStream : DocumentStream, new()
    {
        return CreateStreamAndStoreAsync<TStream>(null, events?.ToList());
    }
    
    public TStream CreateStreamAndStore<TStream>(IEnumerable<Event> events) where TStream : DocumentStream, new()
    {
        return CreateStreamAndStore<TStream>(null, events?.ToList());
    }

    public Task<TStream> CreateStreamAndStoreAsync<TStream>(string streamId, IEnumerable<Event> events)
        where TStream : DocumentStream, new()
    {
        return CreateStreamAndStoreAsync<TStream>(streamId, events?.ToList());
    }
    
    public TStream CreateStreamAndStore<TStream>(string streamId, IEnumerable<Event> events)
        where TStream : DocumentStream, new()
    {
        return CreateStreamAndStore<TStream>(streamId, events?.ToList());
    }
    
    public Task<TStream> CreateStreamAndStoreAsync<TStream>(params Event[] events) where TStream : DocumentStream, new()
    {
        return CreateStreamAndStoreAsync<TStream>(null, events?.ToList());
    }
    
    public TStream CreateStreamAndStore<TStream>(params Event[] events) where TStream : DocumentStream, new()
    {
        return CreateStreamAndStore<TStream>(null, events?.ToList());
    }
    
    public Task<TStream> CreateStreamAndStoreAsync<TStream>(string streamId, params Event[] events)
        where TStream : DocumentStream, new()
    {
        return CreateStreamAndStoreAsync<TStream>(streamId, events?.ToList());
    }
    
    public TStream CreateStreamAndStore<TStream>(string streamId, params Event[] events)
        where TStream : DocumentStream, new()
    {
        return CreateStreamAndStore<TStream>(streamId, events?.ToList());
    }
    
    private async Task<TStream> CreateStreamAndStoreAsync<TStream>(string streamId, List<Event> events) where TStream : DocumentStream, new()
    {
        using (var session = DocumentStore.OpenAsyncSession())
        {
            var stream = await HandleCreateAsync<TStream>(session, streamId, events);
            await session.SaveChangesAsync();
            return stream;
        }
    }
    
    private TStream CreateStreamAndStore<TStream>(string streamId, List<Event> events) where TStream : DocumentStream, new()
    {
        using (var session = DocumentStore.OpenSession())
        {
            var stream = HandleCreate<TStream>(session, streamId, events);
            session.SaveChanges();
            return stream;
        }
    }
}