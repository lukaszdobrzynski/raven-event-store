using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Raven.EventStore;

public interface IRavenEventStore
{
    string DatabaseName { get; }
    Task<TStream> CreateStreamAndStoreAsync<TStream>(params Event[] events)
        where TStream : DocumentStream, new();
    Task<TStream> CreateStreamAndStoreAsync<TStream>(IEnumerable<Event> events, CancellationToken cancellationToken = default)
        where TStream : DocumentStream, new();
    Task<TStream> CreateStreamAndStoreAsync<TStream>(string streamId, params Event[] events)
        where TStream : DocumentStream, new();
    Task<TStream> CreateStreamAndStoreAsync<TStream>(string streamId, IEnumerable<Event> events, CancellationToken cancellationToken = default)
        where TStream : DocumentStream, new();
    TStream CreateStreamAndStore<TStream>(params Event[] events)
        where TStream : DocumentStream, new();
    TStream CreateStreamAndStore<TStream>(IEnumerable<Event> events)
        where TStream : DocumentStream, new();
    TStream CreateStreamAndStore<TStream>(string streamId, params Event[] events)
        where TStream : DocumentStream, new();
    TStream CreateStreamAndStore<TStream>(string streamId, IEnumerable<Event> events)
        where TStream : DocumentStream, new();

    Task AppendAndStoreAsync(Guid streamKey, params Event[] events);
    Task AppendAndStoreAsync(Guid streamKey, IEnumerable<Event> events, CancellationToken cancellationToken = default);
    void AppendAndStore(Guid streamKey, params Event[] events);

    Task<TStream> SliceStreamAndStoreAsync<TStream>(Guid streamKey, params Event[] events)
        where TStream : DocumentStream, new();
    Task<TStream> SliceStreamAndStoreAsync<TStream>(Guid streamKey, string newStreamId, params Event[] events)
        where TStream : DocumentStream, new();
    Task<TStream> SliceStreamAndStoreAsync<TStream>(Guid streamKey, string newStreamId, IEnumerable<Event> events, CancellationToken cancellationToken = default)
        where TStream : DocumentStream, new();
    TStream SliceStreamAndStore<TStream>(Guid streamKey, params Event[] events)
        where TStream : DocumentStream, new();
    TStream SliceStreamAndStore<TStream>(Guid streamKey, string newStreamId, params Event[] events)
        where TStream : DocumentStream, new();

    Task<TStream> GetStreamAsync<TStream>(string streamId, CancellationToken cancellationToken = default)
        where TStream : DocumentStream;
    TStream GetStream<TStream>(string streamId)
        where TStream : DocumentStream;

    Task<TAggregate> GetAggregateAsync<TAggregate>(string streamId, CancellationToken cancellationToken = default)
        where TAggregate : Aggregate;
    TAggregate GetAggregate<TAggregate>(string streamId)
        where TAggregate : Aggregate;

    Task<TAggregate> GetAggregateAtVersionAsync<TAggregate, TStream>(Guid streamKey, int version,
        CancellationToken cancellationToken = default)
        where TAggregate : Aggregate
        where TStream : DocumentStream;

    TAggregate GetAggregateAtVersion<TAggregate, TStream>(Guid streamKey, int version)
        where TAggregate : Aggregate
        where TStream : DocumentStream;

    Task<TAggregate> GetAggregateAtAsync<TAggregate, TStream>(Guid streamKey, DateTime timestamp, CancellationToken cancellationToken = default)
        where TAggregate : Aggregate
        where TStream : DocumentStream;
    TAggregate GetAggregateAt<TAggregate, TStream>(Guid streamKey, DateTime timestamp)
        where TAggregate : Aggregate
        where TStream : DocumentStream;
}
