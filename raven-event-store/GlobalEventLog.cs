namespace Raven.EventStore;

public class GlobalEventLog
{
    public string Id { get; set; }
    public string StreamId { get; set; }
    public string Sequence { get; set; }
    public Event Event { get; set; }
    
    internal static GlobalEventLog From(string sequence, string streamId, Event @event) => 
        new () { Sequence = sequence, StreamId = streamId, Event = @event };
}