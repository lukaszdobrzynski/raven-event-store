using System;
using System.Collections.Generic;
using System.Linq;

namespace Raven.EventStore;

public abstract class DocumentStream
{
    public string Id { get; set; }
    public Guid LogicalId { get; init; }   
    public List<Event> Events { get; set; } = [];
    public int Position => Events.Count != 0 ? Events.Last().Version : 0;
    public DateTime? UpdatedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public Snapshot SeedSnapshot { get; set; }
    public Snapshot ArchivedSnapshot { get; set; }
}