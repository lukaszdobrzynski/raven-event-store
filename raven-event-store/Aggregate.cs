using System;

namespace Raven.EventStore;

public abstract class Aggregate
{
    public string Id { get; internal set; }
    public Guid StreamKey { get; internal set; }
    internal abstract void Build(DocumentStream stream);
}

public abstract class Aggregate<T> : Aggregate where T : DocumentStream
{
    protected abstract void Build(T stream);
    internal override void Build(DocumentStream stream)
    {
        Build((T)stream);
    }
}