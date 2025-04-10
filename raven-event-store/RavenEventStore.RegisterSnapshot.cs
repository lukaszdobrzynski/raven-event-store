using System;

namespace Raven.EventStore;

public partial class RavenEventStore
{
    internal void RegisterSnapshot(Type snapshot)
    {
        if (_settings.Snapshots.TryGetValue(snapshot, out _))
        {
            throw new ArgumentException($"Snapshot of type {snapshot.FullName} is already registered");
        }

        _settings.Snapshots.Add(snapshot);
    }
}