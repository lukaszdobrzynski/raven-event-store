using System;
using System.Collections.Generic;
using Raven.EventStore.Exceptions;

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
        var aggregate = typeof(TAggregate);
        
        ValidateGenericAggregateType(aggregate);
        
        Types.Add(typeof(TAggregate));
    }

    public void Add(Type aggregate)
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        
        ValidateGenericAggregateType(aggregate);
        
        Types.Add(aggregate);
    }
    
    private void ValidateGenericAggregateType(Type aggregate)
    {
        var baseType = aggregate.BaseType;

        if (baseType.IsGenericType == false)
            throw new EventStoreConfigurationException($"Aggregate {aggregate.Name} must inherit from Aggregate<T> where T : {nameof(DocumentStream)}.");

        var genericDefinition = baseType.GetGenericTypeDefinition();

        if (genericDefinition != typeof(Aggregate<>))
            throw new EventStoreConfigurationException($"Aggregate {aggregate.Name} must inherit from Aggregate<T> where T : {nameof(DocumentStream)}.");

        var genericArgument = baseType.GetGenericArguments()[0];

        if (typeof(DocumentStream).IsAssignableFrom(genericArgument) == false)
            throw new EventStoreConfigurationException($"The type argument {genericArgument.Name} in Aggregate {aggregate.Name} must inherit from {nameof(DocumentStream)} abstract class.");
    }
}