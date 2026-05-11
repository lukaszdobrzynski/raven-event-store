using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Raven.Client.Documents.Session;

namespace Raven.EventStore;

public partial class RavenEventStore
{
    private void HandleAppend<TStream>(IDocumentSession session, string streamId, List<Event> events)
        where TStream : DocumentStream
    {
        CheckForNullOrEmptyEvents(events);

        var stream = session
            .Include<TStream>(x => x.SeedId)
            .Include<TStream>(x => x.AggregateId)
            .Load<TStream>(streamId);

        CheckForNonExistentStream(stream, streamId);
        CheckForAttemptToAppendToNonHead(stream);
        AssignVersionToEvents(events, nextVersion: stream.Position + 1);

        var existingSeed = stream.SeedId is not null
            ? session.Load<SliceStreamSeed>(stream.SeedId)
            : null;

        CheckForMissingSeed(existingSeed, stream.Id, stream.SeedId);
        
        var existingAggregate = stream.AggregateId is not null
            ? session.Load<Aggregate>(stream.AggregateId)
            : null;

        CheckForMissingAggregate(existingAggregate, stream.Id, stream.AggregateId);
        
        AddEventsToStream(stream, events);

        var aggregate = existingAggregate is not null
            ? ApplyNewEvents(existingAggregate, events)
            : BuildAggregate(stream, existingSeed?.State);

        if (aggregate is not null)
        {
            aggregate.Id = stream.AggregateId;
            session.Store(aggregate);
        }

        var header = session.Load<StreamHeader>(StreamHeader.GetId(stream.StreamKey));
        header.HeadPosition = stream.Position;

        AppendToGlobalLog(session, streamId, stream.StreamKey, events);
    }

    private async Task HandleAppendAsync<TStream>(IAsyncDocumentSession session, string streamId, List<Event> events, CancellationToken cancellationToken = default)
        where TStream : DocumentStream
    {
        CheckForNullOrEmptyEvents(events);

        var stream = await session
            .Include<TStream>(x => x.SeedId)
            .Include<TStream>(x => x.AggregateId)
            .LoadAsync<TStream>(streamId, cancellationToken);

        CheckForNonExistentStream(stream, streamId);
        CheckForAttemptToAppendToNonHead(stream);
        AssignVersionToEvents(events, nextVersion: stream.Position + 1);

        var existingAggregate = stream.AggregateId is not null
            ? await session.LoadAsync<Aggregate>(stream.AggregateId, cancellationToken)
            : null;

        var seedDoc = stream.SeedId is not null
            ? await session.LoadAsync<SliceStreamSeed>(stream.SeedId, cancellationToken)
            : null;

        CheckForMissingSeed(seedDoc, stream.Id, stream.SeedId);
        CheckForMissingAggregate(existingAggregate, stream.Id, stream.AggregateId);
        AddEventsToStream(stream, events);

        var aggregate = existingAggregate is not null
            ? ApplyNewEvents(existingAggregate, events)
            : BuildAggregate(stream, seedDoc?.State);

        if (aggregate is not null)
        {
            aggregate.Id = stream.AggregateId;
            await session.StoreAsync(aggregate, cancellationToken);
        }

        var header = await session.LoadAsync<StreamHeader>(StreamHeader.GetId(stream.StreamKey), cancellationToken);
        header.HeadPosition = stream.Position;

        await AppendToGlobalLogAsync(session, streamId, stream.StreamKey, events, cancellationToken);
    }

    private static void AddEventsToStream<TStream>(TStream stream, List<Event> events) where TStream : DocumentStream
    {
        stream.Events.AddRange(events);
        stream.UpdatedAt = DateTime.UtcNow;
    }
}
