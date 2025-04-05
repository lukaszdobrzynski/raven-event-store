using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

    private List<IProjection> RunProjections<TStream>(TStream stream) where TStream : DocumentStream
    {
        var projections = GetMatchingProjections(stream);

        var projectionTasks = projections.Select(projection => Task.Run(() =>
        {
            var instance = (IProjection)Activator.CreateInstance(projection);
            instance.Project(stream);
            return instance;
        })).ToArray();
        
        Task.WaitAll(projectionTasks.Select(Task (t) => t).ToArray());

        return projectionTasks.Select(task => task.Result).ToList();
    }
    
    private IEnumerable<Type> GetMatchingProjections<TStream>(TStream stream) where TStream : DocumentStream
    {
        var projectionType = typeof(Projection<>).MakeGenericType(stream.GetType());
        return _settings.Projections.Where(projection => projection.IsSubclassOf(projectionType));
    }
}