using System;
using System.Collections.Generic;

namespace Raven.EventStore;

[method: Newtonsoft.Json.JsonConstructor]
public readonly struct SliceImprint(string sliceId, int firstVersion, DateTime firstTimestamp)
{
    public string SliceId { get; init; } = sliceId;
    public int FirstVersion { get; init; } = firstVersion;
    public DateTime FirstTimestamp { get; init; } = firstTimestamp;

    private static readonly IComparer<SliceImprint> ByTimestamp =
        Comparer<SliceImprint>.Create((a, b) => a.FirstTimestamp.CompareTo(b.FirstTimestamp));

    private static readonly IComparer<SliceImprint> ByVersion =
        Comparer<SliceImprint>.Create((a, b) => a.FirstVersion.CompareTo(b.FirstVersion));

    public static int SearchByVersion(List<SliceImprint> imprints, int version)
    {
        var index = imprints.BinarySearch(new SliceImprint { FirstVersion = version }, ByVersion);
        if (index < 0) index = ~index - 1;
        return index;
    }

    public static int SearchByTimestamp(List<SliceImprint> imprints, DateTime timestamp)
    {
        var index = imprints.BinarySearch(new SliceImprint { FirstTimestamp = timestamp }, ByTimestamp);
        if (index < 0) index = ~index - 1;
        return index;
    }
}
