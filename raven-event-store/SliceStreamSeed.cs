namespace Raven.EventStore;

public class SliceStreamSeed
{
    public string Id { get; set; }
    public Aggregate State { get; set; }
}