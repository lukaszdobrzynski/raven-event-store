namespace Raven.EventStore;

public class StreamSliceSeed
{
    public string Id { get; set; }
    public Aggregate State { get; set; }
}