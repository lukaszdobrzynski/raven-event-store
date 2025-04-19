using System;

namespace Raven.EventStore.Tests.Asserts;

public static class StreamAssert
{
    public static void NotNull<T>(T stream) where T : DocumentStream
    {
        Assert.That(stream, Is.Not.Null);
    }

    public static void Key<T>(T stream, Guid expected) where T : DocumentStream
    {
        Assert.That(stream.StreamKey, Is.EqualTo(expected));
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

    public static void ArchiveNotNull<T>(T stream) where T : DocumentStream
    {
        Assert.That(stream.Archive, Is.Not.Null);
    }

    public static void SeedNotNull<T>(T stream) where T : DocumentStream
    {
        Assert.That(stream.Seed, Is.Not.Null);
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

    public static void IsHead<TStream>(TStream stream) where TStream : DocumentStream
    {
        Assert.That(stream.IsHeadSlice, Is.True);
    }

    public static void IsNotHead<TStream>(TStream stream) where TStream : DocumentStream
    {
        Assert.That(stream.IsHeadSlice, Is.False);
    }

    public static void PreviousSliceId<TStream>(TStream stream, string expected) where TStream : DocumentStream
    {
        Assert.That(stream.PreviousSliceId, Is.EqualTo(expected));
    }
    
    public static void NextSliceId<TStream>(TStream stream, string expected) where TStream : DocumentStream
    {
        Assert.That(stream.NextSliceId, Is.EqualTo(expected));
    }
}