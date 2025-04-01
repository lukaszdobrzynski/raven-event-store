using System;
using System.Linq;
using System.Threading.Tasks;
using Raven.Client.Documents.Session;

namespace Raven.EventStore;

public partial class RavenEventStore
{
    internal void AddProjection(Type projection)
    {
        if (_projections.TryGetValue(projection, out _))
        {
            throw new ArgumentException($"Projection of type {projection.FullName} is already registered");
        }
        
        _projections.Add(projection);
    }

    private async Task RunProjectionAndStoreAsync<TStream>(TStream stream, IAsyncDocumentSession session) where TStream : DocumentStream
    {
        var projectionType = typeof(Projection<>).MakeGenericType(stream.GetType());
        var matchingProjection = _projections.SingleOrDefault(projection => projection.IsSubclassOf(projectionType));

        if (matchingProjection is not null)
        {
            var projection = (IProjection)Activator.CreateInstance(matchingProjection);
            projection.Project(stream);
            await session.StoreAsync(projection);
        }
    }
    
    private void RunProjectionAndStore<TStream>(TStream stream, IDocumentSession session) where TStream : DocumentStream
    {
        var projectionType = typeof(Projection<>).MakeGenericType(stream.GetType());
        var matchingProjection = _projections.SingleOrDefault(projection => projection.IsSubclassOf(projectionType));

        if (matchingProjection is not null)
        {
            var projection = (IProjection)Activator.CreateInstance(matchingProjection);
            projection.Project(stream);
            session.Store(projection);
        }
    }
}