using System;

namespace Raven.EventStore;

public abstract class Aggregate
{
    public string Id { get; internal set; }
    public Guid StreamKey { get; internal set; }
}

public abstract class Aggregate<T> : Aggregate where T : DocumentStream
{
    protected abstract void Build(T stream);
    internal void Rebuild(T stream) => Build(stream);
}
