using System;

namespace Raven.EventStore;

public class GlobalEventLog
{
    public string Id { get; set; }
    public string StreamId { get; set; }
    public Guid StreamKey { get; set; }
    public string Sequence { get; set; }
    public Event Event { get; set; }
    
    internal static GlobalEventLog From(string sequence, string streamId, Guid streamKey, Event @event) => 
        new () { Sequence = sequence, StreamId = streamId, StreamKey = streamKey, Event = @event };
}