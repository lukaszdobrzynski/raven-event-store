using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using RavenEventStoreTestModels.Events;
using RavenEventStoreTestModels.Streams;

namespace Raven.EventStore.Bench.Benchmarks;

[MemoryDiagnoser]
public class SliceBenchmark : RavenEventStoreBenchmarkBase
{
    [Params(100)]
    public int SliceCount { get; set; }

    [Benchmark]
    public UserStream SliceStreamAndStore()
    {
        var stream = RavenEventStore.CreateStreamAndStore<UserStream>(
            UserActivatedEvent.Create);

        for (var i = 0; i < SliceCount; i++)
            stream = RavenEventStore.SliceStreamAndStore<UserStream>(stream.Id,
                UserActivatedEvent.Create);

        return stream;
    }

    [Benchmark]
    public async Task<UserStream> SliceStreamAndStoreAsync()
    {
        var stream = await RavenEventStore.CreateStreamAndStoreAsync<UserStream>(
            UserActivatedEvent.Create);

        for (var i = 0; i < SliceCount; i++)
            stream = await RavenEventStore.SliceStreamAndStoreAsync<UserStream>(stream.Id,
                UserActivatedEvent.Create);

        return stream;
    }
}
