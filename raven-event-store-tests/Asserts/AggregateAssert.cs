using System;

namespace Raven.EventStore.Tests.Asserts;

public static class AggregateAssert
{
    public static void StreamKey<T>(T aggregate, Guid expected) where T : Aggregate
    {
        Assert.That(aggregate.StreamKey, Is.EqualTo(expected));
    }

    public static void AggregateId<T>(T aggregate, string expected) where T : Aggregate
    {
        Assert.That(aggregate.Id, Is.EqualTo(expected));
    }
}