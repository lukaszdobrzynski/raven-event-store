using System;

namespace Raven.EventStore;

public partial class RavenEventStore
{
    internal void AddSnapshot(Type snapshot)
    {
        if (_snapshots.TryGetValue(snapshot, out _))
        {
            throw new ArgumentException($"Snapshot of type {snapshot.FullName} is already registered");
        }
        
        _snapshots.Add(snapshot);
    }
}