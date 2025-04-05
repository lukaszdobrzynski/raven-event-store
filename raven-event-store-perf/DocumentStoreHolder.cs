using System;
using Raven.Client.Documents;

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
    }

    public IDocumentStore GetStore => _store;

    public void Dispose()
    {
        _store?.Dispose();
    }
}
