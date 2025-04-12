using System.Collections.Generic;

namespace Raven.EventStore;

public class PagedResult<T>
{
    public List<T> Items { get; set; }
    public bool HasMore => Items.Count < Total;
    public int Total { get; set; }
}