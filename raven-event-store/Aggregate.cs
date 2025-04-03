namespace Raven.EventStore;

public interface IAggregate
{
    void AggregateEvents(DocumentStream stream);
}

public abstract class Aggregate<T> : IAggregate where T : DocumentStream
{
    public string Id { get; private set; }
    protected abstract void AggregateEvents(T stream);
    protected abstract string GetId(T stream);
    
    public void AggregateEvents(DocumentStream stream)
    {
        Id = GetId((T)stream);
        AggregateEvents((T)stream);
    }
}