using System;
using System.Collections.Generic;

namespace Raven.EventStore;

public abstract class DocumentStream
{
    public string Id { get; set; }
    public List<Event> Events { get; set; } = [];
    public int Position => Events.Count;
    public DateTime? UpdatedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public Aggregate SeedSnapshot { get; set; }
    public Aggregate ArchiveSnapshot { get; set; }
}