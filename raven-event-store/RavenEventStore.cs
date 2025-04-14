using System.Collections.Generic;
using IdGen;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;

namespace Raven.EventStore;

public partial class RavenEventStore
{
    private readonly EventStoreSettings _settings = new();
    private static readonly IdGenerator GlobalEventLogSequentialIdGenerator = new (0);
    
    private IDocumentStore DocumentStore { get; }

    internal RavenEventStore(IDocumentStore documentStore)
    {
        DocumentStore = documentStore;
    }
    
    private IAsyncDocumentSession OpenAsyncSession() => DocumentStore.OpenAsyncSession(_settings.DatabaseName);
    private IDocumentSession OpenSession() => DocumentStore.OpenSession(_settings.DatabaseName);
    
    internal void SetUseGlobalStreamLogging(bool useGlobalStreamLLogging) =>
        _settings.UseGlobalStreamLogging = useGlobalStreamLLogging;

    internal void SetDatabaseName(string databaseName) => _settings.DatabaseName = databaseName;
    
    private static void AssignVersionToEvents(List<Event> events, int nextVersion)
    {
        events.ForEach(e => e.Version = nextVersion++);
    }
}