namespace Raven.EventStore;

public class StreamSliceArchive
{
    public string Id { get; set; }
    public Aggregate State { get; set; }
}
