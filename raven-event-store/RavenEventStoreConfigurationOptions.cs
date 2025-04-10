using System;
using System.Collections.Generic;

namespace Raven.EventStore;

public class RavenEventStoreConfigurationOptions
{
    internal RavenEventStoreConfigurationOptions() {}
    public RavenEventStoreSnapshots Snapshots { get; } = new();
    public bool UseGlobalStreamLogging { get; set; }
}

public class RavenEventStoreSnapshots
{
    internal List<Type> Types { get; } = [];
    internal RavenEventStoreSnapshots() {}

    public void Add<TSnapshot>() where TSnapshot : Snapshot
    {
        Types.Add(typeof(TSnapshot));
    }
}