using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Raven.Client.Documents.Session;
using Raven.EventStore.Exceptions;

namespace Raven.EventStore;

public partial class RavenEventStore
{
    private async Task<TAggregate> HandleGetAggregateAtAsync<TAggregate, TStream>(
        IAsyncDocumentSession session, string streamId, DateTime timestamp, CancellationToken cancellationToken = default)
        where TAggregate : Aggregate
        where TStream : DocumentStream
    {
        if (_aggregatesByStream.TryGetValue(typeof(TStream), out var registeredType) == false || registeredType != typeof(TAggregate))
            throw new UnregisteredAggregateTypeException(typeof(TAggregate));

        var stream = await session
            .Include<TStream>(x => x.SeedId)
            .Include<TStream>(x => x.PreviousSliceId)
            .LoadAsync<TStream>(streamId, cancellationToken);

        if (stream is null)
            return null;

        return await ReplayToTimestampAsync<TAggregate, TStream>(session, stream, timestamp, cancellationToken);
    }

    private static async Task<TAggregate> ReplayToTimestampAsync<TAggregate, TStream>(
        IAsyncDocumentSession session, DocumentStream stream, DateTime timestamp, CancellationToken cancellationToken)
        where TAggregate : Aggregate
        where TStream : DocumentStream
    {
        var events = stream.Events.Where(e => e.Timestamp <= timestamp).ToList();

        if (events.Count == 0)
        {
            if (stream.PreviousSliceId is null)
                return null;

            var previousSlice = await session.LoadAsync<TStream>(stream.PreviousSliceId, cancellationToken);
            CheckForNonExistentStream(previousSlice, stream.PreviousSliceId);
            return await ReplayToTimestampAsync<TAggregate, TStream>(session, previousSlice, timestamp, cancellationToken);
        }

        if (stream.SeedId is not null)
        {
            var seed = await session.LoadAsync<SliceStreamSeed>(stream.SeedId, cancellationToken);
            CheckForMissingSeed(seed, stream.Id, stream.SeedId);
            return BuildAggregateAtVersion<TAggregate>(stream, events, seed.State);
        }

        if (stream.PreviousSliceId is not null)
        {
            var priorEvents = await CollectAllEventsAsync<TStream>(session, stream.PreviousSliceId, cancellationToken);
            priorEvents.AddRange(events);
            events = priorEvents;
        }

        return BuildAggregateAtVersion<TAggregate>(stream, events, null);
    }

    private TAggregate HandleGetAggregateAt<TAggregate, TStream>(
        IDocumentSession session, string streamId, DateTime timestamp)
        where TAggregate : Aggregate
        where TStream : DocumentStream
    {
        if (_aggregatesByStream.TryGetValue(typeof(TStream), out var registeredType) == false || registeredType != typeof(TAggregate))
            throw new UnregisteredAggregateTypeException(typeof(TAggregate));

        var stream = session
            .Include<TStream>(x => x.SeedId)
            .Include<TStream>(x => x.PreviousSliceId)
            .Load<TStream>(streamId);

        if (stream is null)
            return null;

        return ReplayToTimestamp<TAggregate, TStream>(session, stream, timestamp);
    }

    private static TAggregate ReplayToTimestamp<TAggregate, TStream>(
        IDocumentSession session, DocumentStream stream, DateTime timestamp)
        where TAggregate : Aggregate
        where TStream : DocumentStream
    {
        var events = stream.Events.Where(e => e.Timestamp <= timestamp).ToList();

        if (events.Count == 0)
        {
            if (stream.PreviousSliceId is null)
                return null;

            var previousSlice = session.Load<TStream>(stream.PreviousSliceId);
            CheckForNonExistentStream(previousSlice, stream.PreviousSliceId);
            return ReplayToTimestamp<TAggregate, TStream>(session, previousSlice, timestamp);
        }

        if (stream.SeedId is not null)
        {
            var seed = session.Load<SliceStreamSeed>(stream.SeedId);
            CheckForMissingSeed(seed, stream.Id, stream.SeedId);
            return BuildAggregateAtVersion<TAggregate>(stream, events, seed.State);
        }

        if (stream.PreviousSliceId is not null)
        {
            var priorEvents = CollectAllEvents<TStream>(session, stream.PreviousSliceId);
            priorEvents.AddRange(events);
            events = priorEvents;
        }

        return BuildAggregateAtVersion<TAggregate>(stream, events, null);
    }
}
