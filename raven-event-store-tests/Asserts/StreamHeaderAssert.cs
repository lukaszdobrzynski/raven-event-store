using System;

namespace Raven.EventStore.Tests.Asserts;

public class StreamHeaderAssert
{
    public static void SliceDescriptorsCount(StreamHeader streamHeader, int expected)
    {
        Assert.That(streamHeader.Slices, Has.Count.EqualTo(expected));
    }

    public static void SliceDescriptor(StreamHeader streamHeader, int descriptorIndex, string expectedSliceId, int expectedFirstVersion, DateTime expectedFirstTimestamp)
    {
        Assert.That(streamHeader.Slices[descriptorIndex].SliceId, Is.EqualTo(expectedSliceId));
        Assert.That(streamHeader.Slices[descriptorIndex].FirstVersion, Is.EqualTo(expectedFirstVersion));
        Assert.That(streamHeader.Slices[descriptorIndex].FirstTimestamp, Is.EqualTo(expectedFirstTimestamp));
    }

    public static void HeadStreamId(StreamHeader streamHeader, string expected)
    {
        Assert.That(streamHeader.HeadStreamId, Is.EqualTo(expected));
    }

    public static void AggregateId(StreamHeader streamHeader, string expected)
    {
        Assert.That(streamHeader.AggregateId, Is.EqualTo(expected));
    }

    public static void HeadPosition(StreamHeader streamHeader, int expected)
    {
        Assert.That(streamHeader.HeadPosition, Is.EqualTo(expected));
    }

    public static void HeadFirstVersion(StreamHeader streamHeader, int expected)
    {
        Assert.That(streamHeader.HeadFirstVersion, Is.EqualTo(expected));
    }

    public static void HeadFirstTimestamp(StreamHeader streamHeader, DateTime expected)
    {
        Assert.That(streamHeader.HeadFirstTimestamp, Is.EqualTo(expected));
    }
}