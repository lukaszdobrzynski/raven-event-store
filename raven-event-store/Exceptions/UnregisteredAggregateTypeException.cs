using System;

namespace Raven.EventStore.Exceptions;

public class UnregisteredAggregateTypeException(Type aggregateType)
    : Exception($"Aggregate of type {aggregateType.FullName} has not been registered in the event store.");
