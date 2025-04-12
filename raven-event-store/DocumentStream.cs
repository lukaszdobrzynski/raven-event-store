using System;
using System.Collections.Generic;
using System.Linq;

namespace Raven.EventStore;

public abstract class DocumentStream
{
    public string Id { get; set; }
    
    public Guid StreamKey { get; init; }
    public string AggregateId { get; set; }
    public List<Event> Events { get; set; } = [];
    public int Position => Events.Count != 0 ? Events.Last().Version : 0;
    public DateTime? UpdatedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public Aggregate Seed { get; set; }
    public Aggregate Archive { get; set; }
}