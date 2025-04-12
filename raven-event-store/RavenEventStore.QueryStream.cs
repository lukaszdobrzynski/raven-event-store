using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace Raven.EventStore;

public partial class RavenEventStore
{
    public async Task<List<Event>> QueryStreamAsync<TStream>(Guid streamKey, int fromVersion)
        where TStream : DocumentStream
    {
        using (var session = DocumentStore.OpenAsyncSession())
        {
            return await QueryStreamAsync<TStream>(session, streamKey, e => e.Version >= fromVersion);
        }
    }
    
    public List<Event> QueryStream<TStream>(Guid streamKey, int fromVersion)
        where TStream : DocumentStream
    {
        using (var session = DocumentStore.OpenSession())
        {
            return QueryStream<TStream>(session, streamKey, e => e.Version >= fromVersion);
        }
    }
    
    public async Task<List<Event>> QueryStreamAsync<TStream>(Guid streamKey, int fromVersion, int toVersion)
        where TStream : DocumentStream
    {
        using (var session = DocumentStore.OpenAsyncSession())
        {
            return await QueryStreamAsync<TStream>(session, streamKey, e => e.Version >= fromVersion && e.Version <= toVersion);
        }
    }
    
    public List<Event> QueryStream<TStream>(Guid streamKey, int fromVersion, int toVersion)
        where TStream : DocumentStream
    {
        using (var session = DocumentStore.OpenSession())
        {
            return QueryStream<TStream>(session, streamKey, e => e.Version >= fromVersion && e.Version <= toVersion);
        }
    }
    
    public async Task<List<Event>> QueryStreamAsync<TStream>(Guid streamKey, DateTime fromDate)
        where TStream : DocumentStream
    {
        using (var session = DocumentStore.OpenAsyncSession())
        {
            return await QueryStreamAsync<TStream>(session, streamKey, e => e.Timestamp >= fromDate);
        }
    }
    
    public List<Event> QueryStream<TStream>(Guid streamKey, DateTime fromDate)
        where TStream : DocumentStream
    {
        using (var session = DocumentStore.OpenSession())
        {
            return QueryStream<TStream>(session, streamKey, e => e.Timestamp >= fromDate);
        }
    }
    
    public async Task<List<Event>> QueryStreamAsync<TStream>(Guid streamKey, DateTime fromDate, DateTime toDate)
        where TStream : DocumentStream
    {
        using (var session = DocumentStore.OpenAsyncSession())
        {
            return await QueryStreamAsync<TStream>(session, streamKey, e => e.Timestamp >= fromDate && e.Timestamp <= toDate);
        }
    }
    
    public List<Event> QueryStream<TStream>(Guid streamKey, DateTime fromDate, DateTime toDate)
        where TStream : DocumentStream
    {
        using (var session = DocumentStore.OpenSession())
        {
            return QueryStream<TStream>(session, streamKey, e => e.Timestamp >= fromDate && e.Timestamp <= toDate);
        }
    }
    
    public async Task<List<Event>> QueryStreamAsync<TStream,TEvent>(Guid streamKey) 
        where TStream : DocumentStream
        where TEvent : Event
    {
        using (var session = DocumentStore.OpenAsyncSession())
        {
            return await QueryStreamAsync<TStream>(session, streamKey, e => e.Name == typeof(TEvent).Name);
        }
    }
    
    public List<Event> QueryStream<TStream,TEvent>(Guid streamKey) 
        where TStream : DocumentStream
        where TEvent : Event
    {
        using (var session = DocumentStore.OpenSession())
        {
            return QueryStream<TStream>(session, streamKey, e => e.Name == typeof(TEvent).Name);
        }
    }
    
    public async Task<List<Event>> QueryStreamAsync<TStream, TEvent>(Guid streamKey, int fromVersion) 
        where TStream : DocumentStream
        where TEvent : Event
    {
        using (var session = DocumentStore.OpenAsyncSession())
        {
            return await QueryStreamAsync<TStream>(session, streamKey,
                e => e.Name == typeof(TEvent).Name && e.Version >= fromVersion);
        }
    }
    
    public List<Event> QueryStream<TStream, TEvent>(Guid streamKey, int fromVersion) 
        where TStream : DocumentStream
        where TEvent : Event
    {
        using (var session = DocumentStore.OpenSession())
        {
            return QueryStream<TStream>(session, streamKey, e => e.Name == typeof(TEvent).Name && e.Version >= fromVersion);
        }
    }
    
    public async Task<List<Event>> QueryStreamAsync<TStream, TEvent>(Guid streamKey, int fromVersion, int toVersion) 
        where TStream : DocumentStream
        where TEvent : Event
    {
        using (var session = DocumentStore.OpenAsyncSession())
        {
            return await QueryStreamAsync<TStream>(session, streamKey,
                e => e.Name == typeof(TEvent).Name && e.Version >= fromVersion && e.Version <= toVersion);
        }
    }
    
    public List<Event> QueryStream<TStream, TEvent>(Guid streamKey, int fromVersion, int toVersion) 
        where TStream : DocumentStream
        where TEvent : Event
    {
        using (var session = DocumentStore.OpenSession())
        {
            return QueryStream<TStream>(session, streamKey,
                e => e.Name == typeof(TEvent).Name && e.Version >= fromVersion && e.Version <= toVersion);
        }
    }
    
    public async Task<List<Event>> QueryStreamAsync<TStream, TEvent>(Guid streamKey, DateTime fromDate) 
        where TStream : DocumentStream
        where TEvent : Event
    {
        using (var session = DocumentStore.OpenAsyncSession())
        {
            return await QueryStreamAsync<TStream>(session, streamKey,
                e => e.Name == typeof(TEvent).Name && e.Timestamp >= fromDate);
        }
    }
    
    public List<Event> QueryStream<TStream, TEvent>(Guid streamKey, DateTime fromDate) 
        where TStream : DocumentStream
        where TEvent : Event
    {
        using (var session = DocumentStore.OpenSession())
        {
            return QueryStream<TStream>(session, streamKey, e => e.Name == typeof(TEvent).Name && e.Timestamp >= fromDate);
        }
    }
    
    public async Task<List<Event>> QueryStreamAsync<TStream, TEvent>(Guid streamKey, DateTime fromDate, DateTime toDate) 
        where TStream : DocumentStream
        where TEvent : Event
    {
        using (var session = DocumentStore.OpenAsyncSession())
        {
            return await QueryStreamAsync<TStream>(session, streamKey,
                e => e.Name == typeof(TEvent).Name && e.Timestamp >= fromDate && e.Timestamp <= toDate);
        }
    }
    
    public List<Event> QueryStream<TStream, TEvent>(Guid streamKey, DateTime fromDate, DateTime toDate) 
        where TStream : DocumentStream
        where TEvent : Event
    {
        using (var session = DocumentStore.OpenSession())
        {
            return QueryStream<TStream>(session, streamKey, 
                e => e.Name == typeof(TEvent).Name && e.Timestamp >= fromDate && e.Timestamp <= toDate);
        }
    }
    
    private async Task<List<Event>> QueryStreamAsync<TStream>(
        IAsyncDocumentSession session, 
        Guid streamKey, 
        Func<Event, bool> filter)
        where TStream : DocumentStream
    {
        var result = await session.Query<TStream>()
            .Where(s => s.StreamKey == streamKey)
            .Select(s => new
            {
                Events = s.Events.Where(filter).ToList()
            })
            .SingleOrDefaultAsync();

        return result?.Events ?? [];
    }
    
    private List<Event> QueryStream<TStream>(
        IDocumentSession session, 
        Guid streamKey, 
        Func<Event, bool> filter)
        where TStream : DocumentStream
    {
        var result = session.Query<TStream>()
            .Where(s => s.StreamKey == streamKey)
            .Select(s => new
            {
                Events = s.Events.Where(filter).ToList()
            })
            .SingleOrDefault();

        return result?.Events ?? [];
    }
}