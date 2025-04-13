using System.Collections.Generic;
using IdGen;
using Raven.Client.Documents;

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
    
    internal void SetUseGlobalStreamLogging(bool useGlobalStreamLLogging) =>
        _settings.UseGlobalStreamLogging = useGlobalStreamLLogging;

    private static void AssignVersionToEvents(List<Event> events, int nextVersion)
    {
        events.ForEach(e => e.Version = nextVersion++);
    }
}