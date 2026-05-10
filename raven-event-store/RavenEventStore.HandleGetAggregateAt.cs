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
        IAsyncDocumentSession session, Guid streamKey, DateTime timestamp, CancellationToken cancellationToken = default)
        where TAggregate : Aggregate
        where TStream : DocumentStream
    {
        if (_aggregatesByStream.TryGetValue(typeof(TStream), out var registeredType) == false || registeredType != typeof(TAggregate))
            throw new UnregisteredAggregateTypeException(typeof(TAggregate));

        var header = await session
            .Include<StreamHeader>(x => x.HeadStreamId)
            .LoadAsync<StreamHeader>(StreamHeader.GetId(streamKey), cancellationToken);

        if (header is null)
            return null;

        var head = await session.LoadAsync<TStream>(header.HeadStreamId, cancellationToken);

        if (head is null)
            return null;

        return await ReplayToTimestampAsync<TAggregate, TStream>(session, head, header, timestamp, cancellationToken);
    }

    private static async Task<TAggregate> ReplayToTimestampAsync<TAggregate, TStream>(
        IAsyncDocumentSession session, DocumentStream head, StreamHeader header, DateTime timestamp, CancellationToken cancellationToken)
        where TAggregate : Aggregate
        where TStream : DocumentStream
    {
        var targetSlice = await ResolveTargetSliceAsync<TStream>(session, head, header, timestamp, cancellationToken);
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
        IAsyncDocumentSession session, DocumentStream head, StreamHeader header, DateTime timestamp, CancellationToken cancellationToken)
        where TStream : DocumentStream
    {
        if (head.Events[0].Timestamp <= timestamp)
            return head;

        var targetIndex = StreamHeader.SearchByTimestamp(header.Slices, timestamp);
        if (targetIndex < 0)
            return null;

        var targetSlice = await session.LoadAsync<TStream>(header.Slices[targetIndex].SliceId, cancellationToken);
        CheckForNonExistentStream(targetSlice, header.Slices[targetIndex].SliceId);
        return targetSlice;
    }

    private TAggregate HandleGetAggregateAt<TAggregate, TStream>(
        IDocumentSession session, Guid streamKey, DateTime timestamp)
        where TAggregate : Aggregate
        where TStream : DocumentStream
    {
        if (_aggregatesByStream.TryGetValue(typeof(TStream), out var registeredType) == false || registeredType != typeof(TAggregate))
            throw new UnregisteredAggregateTypeException(typeof(TAggregate));

        var header = session
            .Include<StreamHeader>(x => x.HeadStreamId)
            .Load<StreamHeader>(StreamHeader.GetId(streamKey));

        if (header is null)
            return null;

        var head = session.Load<TStream>(header.HeadStreamId);

        if (head is null)
            return null;

        return ReplayToTimestamp<TAggregate, TStream>(session, head, header, timestamp);
    }

    private static TAggregate ReplayToTimestamp<TAggregate, TStream>(
        IDocumentSession session, DocumentStream head, StreamHeader header, DateTime timestamp)
        where TAggregate : Aggregate
        where TStream : DocumentStream
    {
        var targetSlice = ResolveTargetSlice<TStream>(session, head, header, timestamp);
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
        IDocumentSession session, DocumentStream head, StreamHeader header, DateTime timestamp)
        where TStream : DocumentStream
    {
        if (head.Events[0].Timestamp <= timestamp)
            return head;

        var targetIndex = StreamHeader.SearchByTimestamp(header.Slices, timestamp);
        if (targetIndex < 0)
            return null;

        var targetSlice = session.Load<TStream>(header.Slices[targetIndex].SliceId);
        CheckForNonExistentStream(targetSlice, header.Slices[targetIndex].SliceId);
        return targetSlice;
    }
}
