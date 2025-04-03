using System;
using System.Collections.Generic;

namespace Raven.EventStore;

public class RavenEventStoreConfigurationOptions
{
    internal RavenEventStoreConfigurationOptions() {}
    public RavenEventStoreProjections Projections { get; } = new();
    public RavenEventStoreSnapshots Snapshots { get; } = new();
}

public class RavenEventStoreProjections
{
    internal List<Type> Types { get; } = [];
    internal RavenEventStoreProjections() {}

    public void Add<TProjection>() where TProjection : IProjection
    {
        Types.Add(typeof(TProjection));
    }
}

public class RavenEventStoreSnapshots
{
    internal List<Type> Types { get; } = [];
    internal RavenEventStoreSnapshots() {}

    public void Add<TAggregate>() where TAggregate : IAggregate
    {
        Types.Add(typeof(TAggregate));
    }
}