using System;

namespace Raven.EventStore;

public class SliceImprint
{
    public string SliceId { get; init; }
    public int FirstVersion { get; init; }
    public DateTime FirstTimestamp { get; init; }
}
