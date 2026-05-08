using System;
using System.Collections.Generic;

namespace Raven.EventStore;

public class SliceImprint
{
    public string SliceId { get; init; }
    public int FirstVersion { get; init; }
    public DateTime FirstTimestamp { get; init; }

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
