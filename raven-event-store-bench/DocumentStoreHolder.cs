using System;
using Raven.Client.Documents;

namespace Raven.EventStore.Bench;

public class DocumentStoreHolder
{
    private static readonly Lazy<IDocumentStore> LazyStore =
        new (() =>
        {
            var store = new DocumentStore
            {
                Urls = ["http://localhost:8080"],
            };

            return store.Initialize();
        });

    public static IDocumentStore Store => LazyStore.Value;
}