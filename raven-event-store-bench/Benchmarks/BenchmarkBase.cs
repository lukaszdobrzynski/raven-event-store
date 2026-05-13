using BenchmarkDotNet.Attributes;
using Raven.Client.Documents;
using Raven.Client.ServerWide;
using Raven.Client.ServerWide.Operations;
using RavenEventStoreTestModels.Aggregates;

namespace Raven.EventStore.Bench.Benchmarks;

public abstract class BenchmarkBase
{
    private IDocumentStore _documentStore;
    protected IRavenEventStore RavenEventStore { get; private set; }
    protected StreamFactory StreamFactory { get; private set; }

    private const string DatabaseName = "raven-event-store";

    [GlobalSetup]
    public virtual void Setup()
    {
        _documentStore = CreateDocumentStore();
        CreateDatabase(_documentStore);
        RavenEventStore = CreateEventStore(_documentStore);
        StreamFactory = new StreamFactory(RavenEventStore);
    }

    [GlobalCleanup]
    public virtual void Cleanup()
    {
        _documentStore.Maintenance.Server.Send(
            new DeleteDatabasesOperation(DatabaseName, true));

        _documentStore.Dispose();
    }
    
    private static IDocumentStore CreateDocumentStore() => DocumentStoreHolder.Store;
    private static void CreateDatabase(IDocumentStore documentStore) => 
        documentStore.Maintenance.Server.Send(new CreateDatabaseOperation(new DatabaseRecord { DatabaseName = DatabaseName })); 

    private static IRavenEventStore CreateEventStore(IDocumentStore documentStore)
    {
        documentStore.AddEventStore(options =>
        {
            options.DatabaseName = DatabaseName;
            options.Aggregates.Register(registry =>
            {
                registry.Add<User>();
            });
        });
        
        return documentStore.GetEventStore(DatabaseName);
    }
}