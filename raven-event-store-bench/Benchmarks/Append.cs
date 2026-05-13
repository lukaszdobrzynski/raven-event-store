using System;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using RavenEventStoreTestModels.Events;

namespace Raven.EventStore.Bench.Benchmarks;

[MemoryDiagnoser]
public class Append : BenchmarkBase
{
    private Guid _streamKey;

    [Params(1, 10, 100, 1_000)]
    public int EventsToAppend { get; set; }

    [Params(100, 1_000, 10_000)]
    public int EventsInStream { get; set; }

    [IterationSetup]
    public void IterationSetup()
    {
        var stream = StreamFactory.CreateUserStream(EventsInStream, 0);
        _streamKey = stream.StreamKey;
    }

    [Benchmark]
    public void AppendAndStore() =>
        RavenEventStore.AppendAndStore(_streamKey, GenerateBatch());

    [Benchmark]
    public Task AppendAndStoreAsync() =>
        RavenEventStore.AppendAndStoreAsync(_streamKey, GenerateBatch());

    private Event[] GenerateBatch() => Enumerable.Range(0, EventsToAppend)
        .Select(_ => UserRegisteredEvent.Create("event-sorcerer", "john@event-sorcerer.com", "MEMBER"))
        .ToArray<Event>();
}
