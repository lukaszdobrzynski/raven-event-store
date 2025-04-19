using System;

namespace Raven.EventStore;

public class GlobalEventLog
{
    public string Id { get; internal set; }
    public string StreamId { get; private set; }
    public Guid StreamKey { get; private set; }
    public string Sequence { get; private set; }
    public Event Event { get; private set; }
    
    internal static GlobalEventLog From(string sequence, string streamId, Guid streamKey, Event @event) => 
        new () { Sequence = sequence, StreamId = streamId, StreamKey = streamKey, Event = @event };
}