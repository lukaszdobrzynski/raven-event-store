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
            .LoadAsync<TStream>(streamId, cancellationToken);

        if (stream is null)
            return null;

        return await ReplayToTimestampAsync<TAggregate, TStream>(session, stream, timestamp, cancellationToken);
    }

    private static async Task<TAggregate> ReplayToTimestampAsync<TAggregate, TStream>(
        IAsyncDocumentSession session, DocumentStream head, DateTime timestamp, CancellationToken cancellationToken)
        where TAggregate : Aggregate
        where TStream : DocumentStream
    {
        var targetSlice = await ResolveTargetSliceAsync<TStream>(session, head, timestamp, cancellationToken);
        if (targetSlice is null)
            return null;

        var events = targetSlice.Events.Where(e => e.Timestamp <= timestamp).ToList();

        if (events.Count == 0)
            return null;

        if (targetSlice.SeedId is not null)
        {
            var seed = await session.LoadAsync<SliceStreamSeed>(targetSlice.SeedId, cancellationToken);
            CheckForMissingSeed(seed, targetSlice.Id, targetSlice.SeedId);
            return BuildAggregateAtVersion<TAggregate>(targetSlice, events, seed.State);
        }

        return BuildAggregateAtVersion<TAggregate>(targetSlice, events, null);
    }

    private static async Task<DocumentStream> ResolveTargetSliceAsync<TStream>(
        IAsyncDocumentSession session, DocumentStream head, DateTime timestamp, CancellationToken cancellationToken)
        where TStream : DocumentStream
    {
        if (head.Events[0].Timestamp <= timestamp)
            return head;

        var targetIndex = head.PriorSlices.FindLastIndex(e => e.FirstTimestamp <= timestamp);
        if (targetIndex < 0)
            return null;

        var targetSlice = await session.LoadAsync<TStream>(head.PriorSlices[targetIndex].SliceId, cancellationToken);
        CheckForNonExistentStream(targetSlice, head.PriorSlices[targetIndex].SliceId);
        return targetSlice;
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
            .Load<TStream>(streamId);

        if (stream is null)
            return null;

        return ReplayToTimestamp<TAggregate, TStream>(session, stream, timestamp);
    }

    private static TAggregate ReplayToTimestamp<TAggregate, TStream>(
        IDocumentSession session, DocumentStream head, DateTime timestamp)
        where TAggregate : Aggregate
        where TStream : DocumentStream
    {
        var targetSlice = ResolveTargetSlice<TStream>(session, head, timestamp);
        if (targetSlice is null)
            return null;

        var events = targetSlice.Events.Where(e => e.Timestamp <= timestamp).ToList();

        if (events.Count == 0)
            return null;

        if (targetSlice.SeedId is not null)
        {
            var seed = session.Load<SliceStreamSeed>(targetSlice.SeedId);
            CheckForMissingSeed(seed, targetSlice.Id, targetSlice.SeedId);
            return BuildAggregateAtVersion<TAggregate>(targetSlice, events, seed.State);
        }

        return BuildAggregateAtVersion<TAggregate>(targetSlice, events, null);
    }

    private static DocumentStream ResolveTargetSlice<TStream>(
        IDocumentSession session, DocumentStream head, DateTime timestamp)
        where TStream : DocumentStream
    {
        if (head.Events[0].Timestamp <= timestamp)
            return head;

        var targetIndex = head.PriorSlices.FindLastIndex(e => e.FirstTimestamp <= timestamp);
        if (targetIndex < 0)
            return null;

        var targetSlice = session.Load<TStream>(head.PriorSlices[targetIndex].SliceId);
        CheckForNonExistentStream(targetSlice, head.PriorSlices[targetIndex].SliceId);
        return targetSlice;
    }
}
