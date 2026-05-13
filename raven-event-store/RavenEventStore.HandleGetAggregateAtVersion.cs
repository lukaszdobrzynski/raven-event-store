using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Raven.Client.Documents.Session;
using Raven.EventStore.Exceptions;

namespace Raven.EventStore;

public partial class RavenEventStore
{
    private async Task<TAggregate> HandleGetAggregateAtVersionAsync<TAggregate, TStream>(
        IAsyncDocumentSession session, Guid streamKey, int version, CancellationToken cancellationToken = default)
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

        return await ReplayToVersionAsync<TAggregate, TStream>(session, head, header, version, cancellationToken);
    }

    private static async Task<TAggregate> ReplayToVersionAsync<TAggregate, TStream>(
        IAsyncDocumentSession session, DocumentStream head, StreamHeader header, int version, CancellationToken cancellationToken)
        where TAggregate : Aggregate
        where TStream : DocumentStream
    {
        var targetSlice = await ResolveTargetSliceAsync<TStream>(session, head, header, version, cancellationToken);
        if (targetSlice is null)
            return null;

        var events = targetSlice.Events.Where(e => e.Version <= version).ToList();

        if (events.Count == 0 || events[^1].Version != version)
            return null;

        if (targetSlice.SeedId is not null)
        {
            var seed = await session.LoadAsync<StreamSliceSeed>(targetSlice.SeedId, cancellationToken);
            CheckForMissingSeed(seed, targetSlice.Id, targetSlice.SeedId);
            return BuildAggregateAtVersion<TAggregate>(targetSlice, events, seed.State);
        }

        return BuildAggregateAtVersion<TAggregate>(targetSlice, events);
    }

    private static async Task<DocumentStream> ResolveTargetSliceAsync<TStream>(
        IAsyncDocumentSession session, DocumentStream head, StreamHeader header, int version, CancellationToken cancellationToken)
        where TStream : DocumentStream
    {
        if (head.Events[0].Version <= version)
            return head;

        var targetIndex = StreamHeader.SearchByVersion(header.Slices, version);
        if (targetIndex < 0)
            return null;

        var targetSlice = await session.LoadAsync<TStream>(header.Slices[targetIndex].SliceId, cancellationToken);
        CheckForNonExistentStream(targetSlice, header.Slices[targetIndex].SliceId);
        return targetSlice;
    }

    private TAggregate HandleGetAggregateAtVersion<TAggregate, TStream>(
        IDocumentSession session, Guid streamKey, int version)
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

        return ReplayToVersion<TAggregate, TStream>(session, head, header, version);
    }

    private static TAggregate ReplayToVersion<TAggregate, TStream>(
        IDocumentSession session, DocumentStream head, StreamHeader header, int version)
        where TAggregate : Aggregate
        where TStream : DocumentStream
    {
        var targetSlice = ResolveTargetSlice<TStream>(session, head, header, version);
        if (targetSlice is null)
            return null;

        var events = targetSlice.Events.Where(e => e.Version <= version).ToList();

        if (events.Count == 0 || events[^1].Version != version)
            return null;

        if (targetSlice.SeedId is not null)
        {
            var seed = session.Load<StreamSliceSeed>(targetSlice.SeedId);
            CheckForMissingSeed(seed, targetSlice.Id, targetSlice.SeedId);
            return BuildAggregateAtVersion<TAggregate>(targetSlice, events, seed.State);
        }

        return BuildAggregateAtVersion<TAggregate>(targetSlice, events);
    }

    private static DocumentStream ResolveTargetSlice<TStream>(
        IDocumentSession session, DocumentStream head, StreamHeader header, int version)
        where TStream : DocumentStream
    {
        if (head.Events[0].Version <= version)
            return head;

        var targetIndex = StreamHeader.SearchByVersion(header.Slices, version);
        if (targetIndex < 0)
            return null;

        var targetSlice = session.Load<TStream>(header.Slices[targetIndex].SliceId);
        CheckForNonExistentStream(targetSlice, header.Slices[targetIndex].SliceId);
        return targetSlice;
    }

    private static TAggregate BuildAggregateAtVersion<TAggregate>(
        DocumentStream stream, List<Event> events, Aggregate seed = null)
        where TAggregate : Aggregate
    {
        var instance = seed is not null
            ? (TAggregate)seed
            : (TAggregate)Activator.CreateInstance(typeof(TAggregate));

        instance.ApplyEvents(events);
        instance.StreamKey = stream.StreamKey;
        return instance;
    }
}
