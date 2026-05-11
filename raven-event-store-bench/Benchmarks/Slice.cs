using System;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using RavenEventStoreTestModels.Events;
using RavenEventStoreTestModels.Streams;

namespace Raven.EventStore.Bench.Benchmarks;

[MemoryDiagnoser]
public class Slice : BenchmarkBase
{
    private Guid _sourceStreamKey;

    [Params(1, 10, 100)]
    public int EventsInNewSlice { get; set; }

    [Params(100, 1_000, 10_000)]
    public int EventsInSourceStream { get; set; }

    [Params(0, 5, 10)]
    public int SliceChainDepth { get; set; }

    [IterationSetup]
    public void IterationSetup()
    {
        var stream = StreamFactory.CreateUserStream(EventsInSourceStream, SliceChainDepth);
        _sourceStreamKey = stream.StreamKey;
    }

    [Benchmark]
    public UserStream SliceAndStore() =>
        RavenEventStore.SliceStreamAndStore<UserStream>(_sourceStreamKey, GenerateBatch());

    [Benchmark]
    public Task<UserStream> SliceAndStoreAsync() =>
        RavenEventStore.SliceStreamAndStoreAsync<UserStream>(_sourceStreamKey, GenerateBatch());

    private Event[] GenerateBatch() => Enumerable.Range(0, EventsInNewSlice)
        .Select(_ => UserRegisteredEvent.Create("event-sorcerer", "john@event-sorcerer.com", "MEMBER"))
        .ToArray<Event>();
}
