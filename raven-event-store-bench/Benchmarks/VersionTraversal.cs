using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using RavenEventStoreTestModels.Aggregates;
using RavenEventStoreTestModels.Streams;

namespace Raven.EventStore.Bench.Benchmarks;

[MemoryDiagnoser]
public class VersionTraversal : RavenEventStoreBenchmarkBase
{
    private UserStream _stream;
    private int _targetVersion;

    private const int TargetSlice = 2;
    
    [Params(10, 50, 100, 1000)]
    public int EventsPerSlice { get; set; }

    [Params(100, 200, 500, 1000)]
    public int SliceCount { get; set; }

    public override void Setup()
    {
        base.Setup();
        _targetVersion = ((TargetSlice -1) * EventsPerSlice) + (EventsPerSlice / 2);
        _stream = StreamFactory.CreateUserStream(EventsPerSlice, SliceCount);
    }

    [Benchmark]
    public User GetAggregateAtVersion() =>
        RavenEventStore.GetAggregateAtVersion<User, UserStream>(_stream.StreamKey, _targetVersion);

    [Benchmark]
    public Task<User> GetAggregateAtVersionAsync() =>
        RavenEventStore.GetAggregateAtVersionAsync<User, UserStream>(_stream.StreamKey, _targetVersion);
}