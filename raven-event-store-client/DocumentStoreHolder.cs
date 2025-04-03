using System;
using Raven.Client.Documents;

namespace Raven.EventStore.Client;

public class DocumentStoreHolder
{
    private static readonly Lazy<IDocumentStore> LazyStore =
        new (() =>
        {
            var store = new DocumentStore
            {
                Urls = ["http://localhost:8080"],
                Database = "Library"
            };

            store.Initialize();
            store.ConfigureEventStore(options =>
            {
                options.Projections.Add<UserProjection>();
                options.Snapshots.Add<User>();
            });
            
            return store;
        });

    public static IDocumentStore Store => LazyStore.Value;
}
