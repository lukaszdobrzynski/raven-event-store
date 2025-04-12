using System;

namespace Raven.EventStore;

public partial class RavenEventStore
{
    internal void RegisterAggregate(Type aggregate)
    {
        if (_settings.Aggregates.TryGetValue(aggregate, out _))
        {
            throw new ArgumentException($"Aggregate of type {aggregate.FullName} is already registered");
        }

        _settings.Aggregates.Add(aggregate);
    }
}