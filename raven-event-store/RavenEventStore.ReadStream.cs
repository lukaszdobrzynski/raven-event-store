using System.Threading.Tasks;

namespace Raven.EventStore;

public partial class RavenEventStore
{
    public async Task<TStream> ReadStreamAsync<TStream>(string streamId) where TStream : DocumentStream
    {
        using (var session = DocumentStore.OpenAsyncSession())
        {
            return await session.LoadAsync<TStream>(streamId);    
        }
    }
    
    public TStream ReadStream<TStream>(string streamId) where TStream : DocumentStream
    {
        using (var session = DocumentStore.OpenSession())
        {
            return session.Load<TStream>(streamId);    
        }
    }
}