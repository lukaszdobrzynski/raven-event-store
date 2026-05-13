using System;
using System.Collections.Generic;

namespace Raven.EventStore;

public class StreamHeader
{
    public string Id { get; internal set; }
    public string HeadStreamId { get; internal set; }
    public string AggregateId { get; internal set; }
    public string HeadSeedId { get; internal set; }
    public int HeadPosition { get; internal set; }
    public int HeadFirstVersion { get; internal set; }
    public DateTime HeadFirstTimestamp { get; internal set; }
    public Guid StreamKey { get; private set; }
    public List<SliceDescriptor> Slices { get; internal set; } = [];

    internal static StreamHeader Create(
        Guid streamKey,
        string headStreamId,
        string aggregateId,
        int headPosition,
        int headFirstVersion,
        DateTime headFirstTimestamp) => new()
    {
        Id = GetId(streamKey),
        StreamKey = streamKey,
        HeadStreamId = headStreamId,
        AggregateId = aggregateId,
        HeadPosition = headPosition,
        HeadFirstVersion = headFirstVersion,
        HeadFirstTimestamp = headFirstTimestamp,
    };
    
    public void AddSliceDescriptor(string sourceStreamId, int sourceStreamFirstVersion, DateTime sourceStreamFirstTimestamp)
    {
        var slice = new SliceDescriptor
        {
            SliceId = sourceStreamId,
            FirstVersion = sourceStreamFirstVersion,
            FirstTimestamp = sourceStreamFirstTimestamp
        };
        
        Slices.Add(slice);
    }

    internal static string GetId(Guid streamKey) => $"StreamHeaders/{streamKey}";

    private static readonly IComparer<SliceDescriptor> ByTimestamp =
        Comparer<SliceDescriptor>.Create((a, b) => a.FirstTimestamp.CompareTo(b.FirstTimestamp));

    private static readonly IComparer<SliceDescriptor> ByVersion =
        Comparer<SliceDescriptor>.Create((a, b) => a.FirstVersion.CompareTo(b.FirstVersion));

    public static int SearchByVersion(List<SliceDescriptor> descriptors, int version)
    {
        var index = descriptors.BinarySearch(new SliceDescriptor { FirstVersion = version }, ByVersion);
        if (index < 0) index = ~index - 1;
        return index;
    }

    public static int SearchByTimestamp(List<SliceDescriptor> descriptors, DateTime timestamp)
    {
        var index = descriptors.BinarySearch(new SliceDescriptor { FirstTimestamp = timestamp }, ByTimestamp);
        if (index < 0) index = ~index - 1;
        return index;
    }
}