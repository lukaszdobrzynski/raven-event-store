using System;
using System.Linq;
using Raven.Client.Documents;
using Raven.Client.ServerWide;
using Raven.Client.ServerWide.Operations;

namespace Raven.EventStore.Perf;

public class DocumentStoreHolder : IDisposable
{
    private readonly IDocumentStore _store;
    
    public DocumentStoreHolder(string url, string database)
    {
        _store = new DocumentStore
        {
            Urls = [url],
            Database = database
        };

        _store.Initialize();
        CreateDatabaseIfNotExists(_store, database);
    }

    public IDocumentStore GetStore => _store;

    private static void CreateDatabaseIfNotExists(IDocumentStore store, string dbName)
    {
        var databaseNames = store.Maintenance.Server.Send(new GetDatabaseNamesOperation(0, 100)).ToList();

        if (databaseNames.Any(x => x.Equals(dbName)) == false)
        {
            store.Maintenance.Server.Send(new CreateDatabaseOperation(new DatabaseRecord(dbName)));
        }
    }
    
    public void Dispose()
    {
        _store?.Dispose();
    }
}