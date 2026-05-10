using System;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using RavenEventStoreTestModels.Aggregates;
using RavenEventStoreTestModels.Streams;

namespace Raven.EventStore.Bench.Benchmarks;

[MemoryDiagnoser]
public class TimeTraversal : BenchmarkBase
{
    private UserStream _stream;
    private DateTime _targetTimestamp;

    private const int TargetSlice = 2;
    private static readonly DateTime BaseTimestamp = new (2015, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    [Params(10, 50, 100, 1000)]
    public int EventsPerSlice { get; set; }

    [Params(100, 200, 500, 1000)]
    public int SliceCount { get; set; }

    public override void Setup()
    {
        base.Setup();
        var targetEventIndex = ((TargetSlice - 1) * EventsPerSlice) + (EventsPerSlice / 2);
        _targetTimestamp = GetTimestamp(targetEventIndex);
        _stream = StreamFactory.CreateUserStream(EventsPerSlice, SliceCount, GetTimestamp);
    }

    [Benchmark]
    public User GetAggregateAt() =>
        RavenEventStore.GetAggregateAt<User, UserStream>(_stream.StreamKey, _targetTimestamp);

    [Benchmark]
    public Task<User> GetAggregateAtAsync() =>
        RavenEventStore.GetAggregateAtAsync<User, UserStream>(_stream.StreamKey, _targetTimestamp);

    private static DateTime GetTimestamp(int eventIndex) => BaseTimestamp.AddMinutes(eventIndex * 3);
}
