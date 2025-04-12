using System;
using System.Collections.Generic;

namespace Raven.EventStore;

public class RavenEventStoreConfigurationOptions
{
    internal RavenEventStoreConfigurationOptions() {}
    public RavenEventStoreAggregates Aggregates { get; } = new();
    public bool UseGlobalStreamLogging { get; set; }
}

public class RavenEventStoreAggregates
{
    internal List<Type> Types { get; } = [];
    internal RavenEventStoreAggregates() {}

    public void Add<TAggregate>() where TAggregate : Aggregate
    {
        Types.Add(typeof(TAggregate));
    }
}