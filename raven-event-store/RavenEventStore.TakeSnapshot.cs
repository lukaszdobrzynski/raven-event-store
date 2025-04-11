using System;
using System.Linq;
using Newtonsoft.Json;

namespace Raven.EventStore;

public partial class RavenEventStore
{
    private Snapshot TakeSnapshot<TStream>(TStream stream) where TStream : DocumentStream
    {
        var snapshotType = GetSnapshotType<TStream>();
        if (snapshotType == null)
            return null;

        var instance = stream.SeedSnapshot is not null
            ? (Snapshot)JsonConvert.DeserializeObject(
                JsonConvert.SerializeObject(stream.SeedSnapshot),
                snapshotType)
            : (Snapshot)Activator.CreateInstance(snapshotType);

        instance.TakeSnapshot(stream);
        instance.StreamLogicalId = stream.LogicalId;
        return instance;
    }

    private Type GetSnapshotType<TStream>() where TStream : DocumentStream
    {
        var aggregateType = typeof(Snapshot<>).MakeGenericType(typeof(TStream));
        return _settings.Snapshots.SingleOrDefault(x => x.IsSubclassOf(aggregateType));
    }
}