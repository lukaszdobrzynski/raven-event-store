using System;
using System.Collections.Generic;

namespace Raven.EventStore;

public class RavenEventStoreConfigurationOptions
{
    internal RavenEventStoreConfigurationOptions() {}
    public RavenEventStoreAggregates Aggregates { get; } = new();
    public bool UseGlobalStreamLogging { get; set; }
    public string DatabaseName { get; set; }
    public string Name { get; set; }
}

public class RavenEventStoreAggregates
{
    internal RavenEventStoreAggregateRegistry Registry { get; set; }

    internal RavenEventStoreAggregates()
    {
        Registry = new RavenEventStoreAggregateRegistry();
    }

    public void Register(Action<RavenEventStoreAggregateRegistry> aggregateRegistry)
    {
        aggregateRegistry?.Invoke(Registry);
    }
}

public class RavenEventStoreAggregateRegistry
{
    internal List<Type> Types { get; } = [];
    internal RavenEventStoreAggregateRegistry() {}

    public void Add<TAggregate>() where TAggregate : Aggregate
    {
        Types.Add(typeof(TAggregate));
    }
}