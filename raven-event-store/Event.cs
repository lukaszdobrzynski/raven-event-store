using System;

namespace Raven.EventStore;

public abstract class Event
{
    public abstract string Name { get; }
    public Guid EventId { get; set; }
    public DateTime Timestamp { get; set; }
    public int Version { get; set; }
}