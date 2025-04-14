using System;
using System.Linq;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;

namespace Raven.EventStore;

public partial class RavenEventStore
{
    public async Task<TStream> ReadStreamAsync<TStream>(string streamId) where TStream : DocumentStream
    {
        using (var session = OpenAsyncSession())
        {
            return await session.LoadAsync<TStream>(streamId);    
        }
    }
    
    public TStream ReadStream<TStream>(string streamId) where TStream : DocumentStream
    {
        using (var session = OpenSession())
        {
            return session.Load<TStream>(streamId);    
        }
    }

    public async Task<PagedResult<TStream>> ReadStreamAsync<TStream>(Guid streamKey, int pageNumber = 1, int pageSize = 100) where TStream : DocumentStream
    {
        using (var session = OpenAsyncSession())
        {
            var skip = (pageNumber - 1) * pageSize;
            
            var result = await session.Query<TStream>()
                .Where(x => x.StreamKey == streamKey)
                .OrderBy(x => x.Position)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<TStream>()
            {
                Items = result,
                Total = result.Count
            };
        }
    }

    public PagedResult<TStream> ReadStream<TStream>(Guid streamKey, int pageNumber = 1, int pageSize = 100) where TStream : DocumentStream
    {
        using (var session = OpenSession())
        {
            var skip = (pageNumber - 1) * pageSize;
            
            var result = session.Query<TStream>()
                .Where(x => x.StreamKey == streamKey)
                .OrderBy(x => x.Position)
                .Skip(skip)
                .Take(pageSize)
                .ToList();

            return new PagedResult<TStream>()
            {
                Items = result,
                Total = result.Count
            };
        }
    }
}