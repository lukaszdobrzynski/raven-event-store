using System;
using System.Collections.Generic;
using System.Linq;

namespace Raven.EventStore;

public abstract class DocumentStream
{
    public string Id { get; internal init; }
    public Guid StreamKey { get; internal init; }
    public string AggregateId { get; internal set; }
    public List<Event> Events { get; internal init; } = [];
    public int Position => Events.Count != 0 ? Events.Last().Version : 0;
    public DateTime? UpdatedAt { get; internal set; }
    public DateTime CreatedAt { get; internal init; }
    public Aggregate Seed { get; internal init; }
    public Aggregate Archive { get; internal set; }
    public string NextSliceId { get; internal set; }
    public string PreviousSliceId { get; internal init; }
    public bool IsHeadSlice => NextSliceId == null;
}