using System;

namespace Raven.EventStore;

public abstract class Snapshot
{
    internal abstract void TakeSnapshot(DocumentStream stream);
}

public abstract class Snapshot<T> : Snapshot where T : DocumentStream
{
    private Guid _streamLogicalId;
    private string _streamName;
    public string Id => $"{_streamName}/{_streamLogicalId}/{nameof(Snapshot)}";
    
    protected abstract void TakeSnapshot(T stream);
    internal override void TakeSnapshot(DocumentStream stream)
    {
        _streamName = typeof(T).Name;
        _streamLogicalId = stream.LogicalId;
        TakeSnapshot((T)stream);
    }
}