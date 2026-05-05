using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Raven.Client.Documents.Session;
using Raven.EventStore.Exceptions;

namespace Raven.EventStore;

public partial class RavenEventStore
{
    private async Task<TStream> HandleSliceAsync<TStream>(IAsyncDocumentSession session, string sourceStreamId, string newStreamId, List<Event> events, CancellationToken cancellationToken = default) where TStream : DocumentStream, new()
    {
        CheckForNullOrEmptyEvents(events);

        var sourceStream = await session
            .Include<TStream>(x => x.AggregateId)
            .LoadAsync(sourceStreamId, cancellationToken);

        if (sourceStream is null)
            throw new NonExistentStreamException(sourceStreamId);

        CheckForAttemptToCreateSliceStreamFromNonHead(sourceStream);
        AssignVersionToEvents(events, sourceStream.Position + 1);

        var aggregate = await session.LoadAsync<Aggregate>(sourceStream.AggregateId, cancellationToken);

        CheckForMissingAggregate(aggregate, sourceStream.Id, sourceStream.AggregateId);

        string seedId = null;
        Aggregate seedForRebuild = null;

        if (aggregate is not null)
        {
            var archiveDoc = new SliceStreamArchive { State = aggregate };
            await session.StoreAsync(archiveDoc, cancellationToken);
            sourceStream.ArchiveId = archiveDoc.Id;

            var seedDoc = new SliceStreamSeed { State = aggregate };
            await session.StoreAsync(seedDoc, cancellationToken);
            seedId = seedDoc.Id;
            seedForRebuild = AggregateCloner.Clone(aggregate);
        }

        var newStreamSlice = CreateNewStreamSlice<TStream>(sourceStreamId, newStreamId, sourceStream.StreamKey, sourceStream.AggregateId, seedId, events, BuildPriorSliceImprints(sourceStream));
        var newAggregate = BuildAggregate(newStreamSlice, seedForRebuild);

        if (aggregate is not null)
        {
            var changeVector = session.Advanced.GetChangeVectorFor(aggregate);
            session.Advanced.Evict(aggregate);
            await session.StoreAsync(newAggregate, changeVector, aggregate.Id, cancellationToken);
        }

        await session.StoreAsync(newStreamSlice, cancellationToken);
        sourceStream.NextSliceId = newStreamSlice.Id;
        await session.StoreAsync(sourceStream, cancellationToken);

        await AppendToGlobalLogAsync(session, newStreamSlice.Id, newStreamSlice.StreamKey, events, cancellationToken);
        return newStreamSlice;
    }

    private TStream HandleSlice<TStream>(IDocumentSession session, string sourceStreamId, string newStreamId, List<Event> events) where TStream : DocumentStream, new()
    {
        CheckForNullOrEmptyEvents(events);

        var sourceStream = session
            .Include<TStream>(x => x.AggregateId)
            .Load(sourceStreamId);

        if (sourceStream is null)
            throw new NonExistentStreamException(sourceStreamId);

        CheckForAttemptToCreateSliceStreamFromNonHead(sourceStream);
        AssignVersionToEvents(events, sourceStream.Position + 1);

        var aggregate = session.Load<Aggregate>(sourceStream.AggregateId);

        CheckForMissingAggregate(aggregate, sourceStream.Id, sourceStream.AggregateId);

        string seedId = null;
        Aggregate seedForRebuild = null;

        if (aggregate is not null)
        {
            var archiveDoc = new SliceStreamArchive { State = aggregate };
            session.Store(archiveDoc);
            sourceStream.ArchiveId = archiveDoc.Id;

            var seedDoc = new SliceStreamSeed { State = aggregate };
            session.Store(seedDoc);
            seedId = seedDoc.Id;
            seedForRebuild = AggregateCloner.Clone(aggregate);
        }

        var newStreamSlice = CreateNewStreamSlice<TStream>(sourceStreamId, newStreamId, sourceStream.StreamKey, sourceStream.AggregateId, seedId, events, BuildPriorSliceImprints(sourceStream));
        var newAggregate = BuildAggregate(newStreamSlice, seedForRebuild);

        if (aggregate is not null)
        {
            var changeVector = session.Advanced.GetChangeVectorFor(aggregate);
            session.Advanced.Evict(aggregate);
            session.Store(newAggregate, changeVector, aggregate.Id);
        }

        session.Store(newStreamSlice);
        sourceStream.NextSliceId = newStreamSlice.Id;
        session.Store(sourceStream);

        AppendToGlobalLog(session, newStreamSlice.Id, newStreamSlice.StreamKey, events);
        return newStreamSlice;
    }

    private static TStream CreateNewStreamSlice<TStream>(string previousStreamId, string newStreamId, Guid streamKey, string aggregateId, string seedId, List<Event> events, List<SliceImprint> priorSlices) where TStream : DocumentStream, new()
    {
        return new TStream
        {
            Id = newStreamId,
            CreatedAt = DateTime.UtcNow,
            Events = events,
            StreamKey = streamKey,
            AggregateId = aggregateId,
            SeedId = seedId,
            PreviousSliceId = previousStreamId,
            PriorSlices = priorSlices
        };
    }

    private static List<SliceImprint> BuildPriorSliceImprints(DocumentStream sourceStream)
    {
        var imprint = new SliceImprint
        {
            SliceId = sourceStream.Id,
            FirstVersion = sourceStream.Events[0].Version,
            FirstTimestamp = sourceStream.Events[0].Timestamp
        };
        return [..sourceStream.PriorSlices, imprint];
    }
}
