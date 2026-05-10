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
}