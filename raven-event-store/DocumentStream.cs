using System;
using System.Collections.Generic;

namespace Raven.EventStore;

public abstract class DocumentStream
{
    public string Id { get; internal init; }
    public Guid StreamKey { get; internal init; }
    public string AggregateId { get; internal set; }
    public List<Event> Events { get; internal init; } = [];
    public int Position { get; internal set; }
    public DateTime? UpdatedAt { get; internal set; }
    public DateTime CreatedAt { get; internal init; }
    public string SeedId { get; internal set; }
    public string ArchiveId { get; internal set; }
    public string NextSliceId { get; internal set; }
    public string PreviousSliceId { get; internal init; }
    public bool IsHeadSlice => NextSliceId == null;
}