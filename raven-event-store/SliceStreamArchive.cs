namespace Raven.EventStore;

public class SliceStreamArchive
{
    public string Id { get; set; }
    public Aggregate State { get; set; }
}
