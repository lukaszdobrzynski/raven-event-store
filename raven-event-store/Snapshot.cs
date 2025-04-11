using System;

namespace Raven.EventStore;

public abstract class Snapshot
{
    public string Id { get; set; }
    public Guid StreamLogicalId { get; set; }
    internal abstract void TakeSnapshot(DocumentStream stream);
}

public abstract class Snapshot<T> : Snapshot where T : DocumentStream
{
    protected abstract void TakeSnapshot(T stream);
    internal override void TakeSnapshot(DocumentStream stream)
    {
        TakeSnapshot((T)stream);
    }
}