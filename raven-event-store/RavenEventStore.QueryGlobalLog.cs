using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace Raven.EventStore;

public partial class RavenEventStore
{
    public async Task<List<GlobalEventLog>> QueryGlobalLogAsync<TEvent>() where TEvent : Event
    {
        using (var session = DocumentStore.OpenAsyncSession())
        {
            return await QueryGlobalLogAsync(session, x => x.Event.Name == typeof(TEvent).Name);
        }
    }
    
    public List<GlobalEventLog> QueryGlobalLog<TEvent>() where TEvent : Event
    {
        using (var session = DocumentStore.OpenSession())
        {
            return QueryGlobalLog(session, x => x.Event.Name == typeof(TEvent).Name);
        }
    }
    
    public async Task<List<GlobalEventLog>> QueryGlobalLogAsync(DateTime fromDate)
    {
        using (var session = DocumentStore.OpenAsyncSession())
        {
            return await QueryGlobalLogAsync(session, x => x.Event.Timestamp >= fromDate);
        }
    }
    
    public List<GlobalEventLog> QueryGlobalLog(DateTime fromDate)
    {
        using (var session = DocumentStore.OpenSession())
        {
            return QueryGlobalLog(session, x => x.Event.Timestamp >= fromDate);
        }
    }
    
    public async Task<List<GlobalEventLog>> QueryGlobalLogAsync(DateTime fromDate, DateTime toDate)
    {
        using (var session = DocumentStore.OpenAsyncSession())
        {
            return await QueryGlobalLogAsync(session, x => x.Event.Timestamp >= fromDate && x.Event.Timestamp <= toDate);
        }
    }
    
    public List<GlobalEventLog> QueryGlobalLog(DateTime fromDate, DateTime toDate)
    {
        using (var session = DocumentStore.OpenSession())
        {
            return QueryGlobalLog(session, x => x.Event.Timestamp >= fromDate && x.Event.Timestamp <= toDate);
        }
    }

    private async Task<List<GlobalEventLog>> QueryGlobalLogAsync(IAsyncDocumentSession session, Expression<Func<GlobalEventLog, bool>> filter)
    {
        return await session.Query<GlobalEventLog>()
            .Where(filter)
            .ToListAsync();
    }
    
    private List<GlobalEventLog> QueryGlobalLog(IDocumentSession session, Func<GlobalEventLog, bool> filter)
    {
        return session.Query<GlobalEventLog>()
            .Where(filter)
            .ToList();
    }
}