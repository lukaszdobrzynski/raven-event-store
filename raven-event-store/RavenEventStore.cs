using System.Collections.Generic;
using IdGen;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;

namespace Raven.EventStore;

public partial class RavenEventStore
{
    internal readonly EventStoreSettings Settings = new();
    private static readonly IdGenerator GlobalEventLogSequentialIdGenerator = new (0);
    public string Name { get; }

    private IDocumentStore DocumentStore { get; }

    internal RavenEventStore(IDocumentStore documentStore, string name)
    {
        DocumentStore = documentStore;
        Name = name;
    }
    
    private IAsyncDocumentSession OpenAsyncSession() => DocumentStore.OpenAsyncSession(Settings.DatabaseName);
    private IDocumentSession OpenSession() => DocumentStore.OpenSession(Settings.DatabaseName);
    
    internal void SetUseGlobalStreamLogging(bool useGlobalStreamLLogging) =>
        Settings.UseGlobalStreamLogging = useGlobalStreamLLogging;

    internal void SetDatabaseName(string databaseName) => Settings.DatabaseName = databaseName;
    
    private static void AssignVersionToEvents(List<Event> events, int nextVersion)
    {
        events.ForEach(e => e.Version = nextVersion++);
    }
}