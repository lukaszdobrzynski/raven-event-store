using System;
using System.Collections.Generic;

namespace Raven.EventStore;

public abstract class Aggregate
{
    public string Id { get; internal set; }
    public Guid StreamKey { get; internal set; }

    protected abstract void Apply(Event evt);

    internal void ApplyEvents(IEnumerable<Event> events)
    {
        foreach (var evt in events) Apply(evt);
    }
}

public abstract class Aggregate<T> : Aggregate where T : DocumentStream
{
}
