namespace Raven.EventStore;

public abstract class Aggregate
{
    public string Id { get; set; }
    public abstract void AggregateEvents(DocumentStream stream);
}

public abstract class Aggregate<T> : Aggregate where T : DocumentStream
{
    protected abstract void AggregateEvents(T stream);
    public override void AggregateEvents(DocumentStream stream)
    {
        AggregateEvents((T)stream);
    }
}