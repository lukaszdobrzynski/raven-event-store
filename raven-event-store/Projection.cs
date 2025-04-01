namespace Raven.EventStore;

public interface IProjection
{
    void Project(DocumentStream stream);
}

public abstract class Projection<T> : IProjection where T : DocumentStream
{
    public string Id { get; private set; }
    protected abstract void Project(T stream);
    protected abstract string GetId(T stream);

    public void Project(DocumentStream stream)
    {
        Id = GetId((T)stream);
        Project((T)stream);   
    }
}