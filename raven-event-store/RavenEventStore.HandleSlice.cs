using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Raven.Client.Documents.Session;

namespace Raven.EventStore;

public partial class RavenEventStore
{
    private async Task<TStream> HandleSliceAsync<TStream>(IAsyncDocumentSession session, Guid streamKey, string newStreamId, List<Event> events, CancellationToken cancellationToken = default) where TStream : DocumentStream, new()
    {
        CheckForNullOrEmptyEvents(events);

        var header = await session
            .Include<StreamHeader>(x => x.AggregateId)
            .LoadAsync<StreamHeader>(StreamHeader.GetId(streamKey), cancellationToken);

        CheckForMissingHeader(header, streamKey);

        AssignVersionToEvents(events, header.HeadPosition + 1);

        var aggregate = header.AggregateId is not null
            ? await session.LoadAsync<Aggregate>(header.AggregateId, cancellationToken)
            : null;

        CheckForMissingAggregate(aggregate, header.HeadStreamId, header.AggregateId);

        string seedId = null;
        Aggregate seedForRebuild = null;
        string archiveId = null;

        if (aggregate is not null)
        {
            var archiveDoc = new StreamSliceArchive { State = aggregate };
            await session.StoreAsync(archiveDoc, cancellationToken);
            archiveId = archiveDoc.Id;

            var seedDoc = new StreamSliceSeed { State = aggregate };
            await session.StoreAsync(seedDoc, cancellationToken);
            seedId = seedDoc.Id;
            seedForRebuild = AggregateCloner.Clone(aggregate);
        }

        var newStreamSlice = CreateNewStreamSlice<TStream>(header.HeadStreamId, newStreamId, header.StreamKey, header.AggregateId, seedId, events);
        var newAggregate = BuildAggregate(newStreamSlice, seedForRebuild);

        if (aggregate is not null)
        {
            var changeVector = session.Advanced.GetChangeVectorFor(aggregate);
            session.Advanced.Evict(aggregate);
            await session.StoreAsync(newAggregate, changeVector, aggregate.Id, cancellationToken);
        }

        await session.StoreAsync(newStreamSlice, cancellationToken);

        session.Advanced.Patch<TStream, string>(header.HeadStreamId, x => x.NextSliceId, newStreamSlice.Id);

        if (archiveId is not null)
        {
            session.Advanced.Patch<TStream, string>(header.HeadStreamId, x => x.ArchiveId, archiveId);
        }
            
        UpdateSliceHeader(header, newStreamSlice, seedId, events);

        await AppendToGlobalLogAsync(session, newStreamSlice.Id, newStreamSlice.StreamKey, events, cancellationToken);
        return newStreamSlice;
    }

    private TStream HandleSlice<TStream>(IDocumentSession session, Guid streamKey, string newStreamId, List<Event> events) where TStream : DocumentStream, new()
    {
        CheckForNullOrEmptyEvents(events);

        var header = session
            .Include<StreamHeader>(x => x.AggregateId)
            .Load<StreamHeader>(StreamHeader.GetId(streamKey));

        CheckForMissingHeader(header, streamKey);

        AssignVersionToEvents(events, header.HeadPosition + 1);

        var aggregate = header.AggregateId is not null
            ? session.Load<Aggregate>(header.AggregateId)
            : null;

        CheckForMissingAggregate(aggregate, header.HeadStreamId, header.AggregateId);

        string seedId = null;
        Aggregate seedForRebuild = null;
        string archiveId = null;

        if (aggregate is not null)
        {
            var archiveDoc = new StreamSliceArchive { State = aggregate };
            session.Store(archiveDoc);
            archiveId = archiveDoc.Id;

            var seedDoc = new StreamSliceSeed { State = aggregate };
            session.Store(seedDoc);
            seedId = seedDoc.Id;
            seedForRebuild = AggregateCloner.Clone(aggregate);
        }

        var newStreamSlice = CreateNewStreamSlice<TStream>(header.HeadStreamId, newStreamId, header.StreamKey, header.AggregateId, seedId, events);
        var newAggregate = BuildAggregate(newStreamSlice, seedForRebuild);

        if (aggregate is not null)
        {
            var changeVector = session.Advanced.GetChangeVectorFor(aggregate);
            session.Advanced.Evict(aggregate);
            session.Store(newAggregate, changeVector, aggregate.Id);
        }

        session.Store(newStreamSlice);

        session.Advanced.Patch<TStream, string>(header.HeadStreamId, x => x.NextSliceId, newStreamSlice.Id);

        if (archiveId is not null)
        {
            session.Advanced.Patch<TStream, string>(header.HeadStreamId, x => x.ArchiveId, archiveId);
        }
            
        UpdateSliceHeader(header, newStreamSlice, seedId, events);

        AppendToGlobalLog(session, newStreamSlice.Id, newStreamSlice.StreamKey, events);
        return newStreamSlice;
    }

    private static void UpdateSliceHeader<TStream>(StreamHeader header, TStream newStreamSlice, string seedId, List<Event> events) where TStream : DocumentStream
    {
        header.AddSliceDescriptor(header.HeadStreamId, header.HeadFirstVersion, header.HeadFirstTimestamp);
        header.HeadStreamId = newStreamSlice.Id;
        header.AggregateId = newStreamSlice.AggregateId;
        header.HeadSeedId = seedId;
        header.HeadPosition = newStreamSlice.Position;
        header.HeadFirstVersion = events[0].Version;
        header.HeadFirstTimestamp = events[0].Timestamp;
    }

    private static TStream CreateNewStreamSlice<TStream>(string previousStreamId, string newStreamId, Guid streamKey, string aggregateId, string seedId, List<Event> events) where TStream : DocumentStream, new()
    {
        return new TStream
        {
            Id = newStreamId,
            CreatedAt = DateTime.UtcNow,
            Events = events,
            Position = events[^1].Version,
            StreamKey = streamKey,
            AggregateId = aggregateId,
            SeedId = seedId,
            PreviousSliceId = previousStreamId,
        };
    }
}
