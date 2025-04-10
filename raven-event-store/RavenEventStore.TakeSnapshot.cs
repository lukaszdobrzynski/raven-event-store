using System;
using System.Linq;
using Newtonsoft.Json;

namespace Raven.EventStore;

public partial class RavenEventStore
{
    private Snapshot TakeSnapshot<TStream>(TStream stream) where TStream : DocumentStream
    {
        var aggregateType = GetSnapshotType<TStream>();
        if (aggregateType == null)
            return null;

        var instance = stream.SeedSnapshot is not null
            ? (Snapshot)JsonConvert.DeserializeObject(
                JsonConvert.SerializeObject(stream.SeedSnapshot),
                aggregateType)
            : (Snapshot)Activator.CreateInstance(aggregateType);

        instance.TakeSnapshot(stream);
        return instance;
    }

    private Type GetSnapshotType<TStream>() where TStream : DocumentStream
    {
        var aggregateType = typeof(Snapshot<>).MakeGenericType(typeof(TStream));
        return _settings.Snapshots.SingleOrDefault(x => x.IsSubclassOf(aggregateType));
    }
}