using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Raven.Client.Documents.Session;

namespace Raven.EventStore;

public partial class RavenEventStore
{
    internal void AddProjection(Type projection)
    {
        if (_settings.Projections.TryGetValue(projection, out _))
        {
            throw new ArgumentException($"Projection of type {projection.FullName} is already registered");
        }
        
        _settings.Projections.Add(projection);
    }

    private async Task RunProjectionsAndStoreAsync<TStream>(TStream stream, IAsyncDocumentSession session) where TStream : DocumentStream
    {
        var projections = GetMatchingProjections(stream);
        
        foreach (var projection in projections)
        {
            var instance = (IProjection)Activator.CreateInstance(projection);
            instance.Project(stream);
            await session.StoreAsync(instance);
        }
    }
    
    private void RunProjectionsAndStore<TStream>(TStream stream, IDocumentSession session) where TStream : DocumentStream
    {
        var projections = GetMatchingProjections(stream);

        foreach (var projection in projections)
        {
            var instance = (IProjection)Activator.CreateInstance(projection);
            instance.Project(stream);
            session.Store(instance);
        }
    }

    private IEnumerable<Type> GetMatchingProjections<TStream>(TStream stream) where TStream : DocumentStream
    {
        var projectionType = typeof(Projection<>).MakeGenericType(stream.GetType());
        return _settings.Projections.Where(projection => projection.IsSubclassOf(projectionType));
    }
}