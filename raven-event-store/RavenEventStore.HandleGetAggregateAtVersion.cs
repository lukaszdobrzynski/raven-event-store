using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Raven.Client.Documents.Session;

namespace Raven.EventStore;

public partial class RavenEventStore
{
    private async Task<TAggregate> HandleGetAggregateAtVersionAsync<TAggregate, TStream>(
        IAsyncDocumentSession session, string streamId, int version, CancellationToken cancellationToken = default)
        where TAggregate : Aggregate
        where TStream : DocumentStream
    {
        var stream = await session
            .Include<TStream>(x => x.SeedId)
            .Include<TStream>(x => x.PreviousSliceId)
            .LoadAsync<TStream>(streamId, cancellationToken);
        
        if (stream is null)
            return null;

        return await ReplayToVersionAsync<TAggregate, TStream>(session, stream, version, cancellationToken);
    }

    private static async Task<TAggregate> ReplayToVersionAsync<TAggregate, TStream>(
        IAsyncDocumentSession session, DocumentStream stream, int version, CancellationToken cancellationToken)
        where TAggregate : Aggregate
        where TStream : DocumentStream
    {
        var events = stream.Events.Where(e => e.Version <= version).ToList();

        if (events.Count == 0)
        {
            if (stream.PreviousSliceId is null)
                return null;

            var previousSlice = await session.LoadAsync<TStream>(stream.PreviousSliceId, cancellationToken);
            CheckForNonExistentStream(previousSlice, stream.PreviousSliceId);
            return await ReplayToVersionAsync<TAggregate, TStream>(session, previousSlice, version, cancellationToken);
        }

        var requestedEventVersionExists = events[^1].Version == version;
        
        if (requestedEventVersionExists == false)
            return null;

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

    private static async Task<List<Event>> CollectAllEventsAsync<TStream>(
        IAsyncDocumentSession session, string sliceId, CancellationToken cancellationToken) where TStream : DocumentStream
    {
        var slice = await session.LoadAsync<TStream>(sliceId, cancellationToken);
        CheckForNonExistentStream(slice, sliceId);

        if (slice.PreviousSliceId is null)
            return [..slice.Events];

        var priorEvents = await CollectAllEventsAsync<TStream>(session, slice.PreviousSliceId, cancellationToken);
        priorEvents.AddRange(slice.Events);
        return priorEvents;
    }

    private TAggregate HandleGetAggregateAtVersion<TAggregate, TStream>(
        IDocumentSession session, string streamId, int version)
        where TAggregate : Aggregate
        where TStream : DocumentStream
    {
        var stream = session
            .Include<TStream>(x => x.SeedId)
            .Include<TStream>(x => x.PreviousSliceId)
            .Load<TStream>(streamId);

        if (stream is null)
            return null;

        return ReplayToVersion<TAggregate, TStream>(session, stream, version);
    }

    private static TAggregate ReplayToVersion<TAggregate, TStream>(
        IDocumentSession session, DocumentStream stream, int version)
        where TAggregate : Aggregate
        where TStream : DocumentStream
    {
        var events = stream.Events.Where(e => e.Version <= version).ToList();

        if (events.Count == 0)
        {
            if (stream.PreviousSliceId is null)
                return null;

            var previousSlice = session.Load<TStream>(stream.PreviousSliceId);
            CheckForNonExistentStream(previousSlice, stream.PreviousSliceId);
            return ReplayToVersion<TAggregate, TStream>(session, previousSlice, version);
        }

        var requestedEventVersionExists = events[^1].Version == version;

        if (requestedEventVersionExists == false)
            return null;

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

    private static List<Event> CollectAllEvents<TStream>(
        IDocumentSession session, string sliceId)
        where TStream : DocumentStream
    {
        var slice = session.Load<TStream>(sliceId);
        CheckForNonExistentStream(slice, sliceId);

        if (slice.PreviousSliceId is null)
            return [..slice.Events];

        var priorEvents = CollectAllEvents<TStream>(session, slice.PreviousSliceId);
        priorEvents.AddRange(slice.Events);
        return priorEvents;
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
