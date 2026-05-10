using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using RavenEventStoreTestModels.Events;
using RavenEventStoreTestModels.Streams;

namespace Raven.EventStore.Bench.Benchmarks;

[MemoryDiagnoser]
public class Append : BenchmarkBase
{
    private string _streamId;

    [Params(1, 10, 100, 1_000)]
    public int EventsToAppend { get; set; }

    [Params(100, 1_000, 10_000)]
    public int EventsInStream { get; set; }

    [IterationSetup]
    public void IterationSetup()
    {
        var stream = StreamFactory.CreateUserStream(EventsInStream, 0);
        _streamId = stream.Id;
    }

    [Benchmark]
    public void AppendAndStore() =>
        RavenEventStore.AppendAndStore<UserStream>(_streamId, GenerateBatch());

    [Benchmark]
    public Task AppendAndStoreAsync() =>
        RavenEventStore.AppendAndStoreAsync<UserStream>(_streamId, GenerateBatch());

    private Event[] GenerateBatch() => Enumerable.Range(0, EventsToAppend)
        .Select(_ => UserRegisteredEvent.Create("event-sorcerer", "john@event-sorcerer.com", "MEMBER"))
        .ToArray<Event>();
}
