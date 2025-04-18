using System;

namespace Raven.EventStore.Tests.Asserts;

public static class StreamAssert
{
    public static void NotNull<T>(T stream) where T : DocumentStream
    {
        Assert.That(stream, Is.Not.Null);
    }

    public static void KeyNotEmpty<T>(T stream) where T : DocumentStream
    {
        Assert.That(stream.StreamKey, Is.Not.EqualTo(Guid.Empty));
    }
    
    public static void Position<T>(T stream, int expected) where T : DocumentStream
    {
        Assert.That(stream.Position, Is.EqualTo(expected));
    }

    public static void EventsNotEmpty<T>(T stream) where T : DocumentStream
    {
        Assert.That(stream.Events, Is.Not.Empty);
    }
    
    public static void ArchiveNull<T>(T stream) where T : DocumentStream
    {
        Assert.That(stream.Archive, Is.Null);   
    }
    
    public static void SeedNull<T>(T stream) where T : DocumentStream
    {
        Assert.That(stream.Seed, Is.Null);   
    }
    
    public static void AggregateIdNull<T>(T stream) where T : DocumentStream
    {
        Assert.That(stream.AggregateId, Is.Null);
    }
    
    public static void EventsCount<T>(T stream, int expected) where T : DocumentStream
    {
        Assert.That(stream.Events, Has.Count.EqualTo(expected));
    }

    public static void IdNotNull<T>(T stream) where T : DocumentStream
    {
        Assert.That(stream.Id, Is.Not.Null);
    }

    public static void AggregateIdNotNull<T>(T stream) where T : DocumentStream
    {
        Assert.That(stream.AggregateId, Is.Not.Null);
    }
}