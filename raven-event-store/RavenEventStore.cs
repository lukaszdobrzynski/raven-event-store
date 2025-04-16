using System;
using System.Collections.Generic;
using IdGen;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;

namespace Raven.EventStore;

public partial class RavenEventStore
{
    private static readonly IdGenerator GlobalEventLogSequentialIdGenerator = new (0);
    public string Name { get; }
    internal string DatabaseName { get; private set; }
    private bool UseGlobalStreamLogging { get; set; }
    
    private readonly HashSet<Type> _aggregates = [];
    private IDocumentStore DocumentStore { get; }

    internal RavenEventStore(IDocumentStore documentStore, string name)
    {
        DocumentStore = documentStore;
        Name = name;
    }
    
    private IAsyncDocumentSession OpenAsyncSession() => DocumentStore.OpenAsyncSession(DatabaseName);
    private IDocumentSession OpenSession() => DocumentStore.OpenSession(DatabaseName);
    
    internal void SetUseGlobalStreamLogging(bool useGlobalStreamLLogging) =>
        UseGlobalStreamLogging = useGlobalStreamLLogging;

    internal void SetDatabaseName(string databaseName) => DatabaseName = databaseName;
    private static void AssignVersionToEvents(List<Event> events, int nextVersion)
    {
        events.ForEach(e => e.Version = nextVersion++);
    }
}