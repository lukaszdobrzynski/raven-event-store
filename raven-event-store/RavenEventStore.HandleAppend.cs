using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Raven.Client.Documents.Commands.Batches;
using Raven.Client.Documents.Session;
using Raven.EventStore.Exceptions;

namespace Raven.EventStore;

public partial class RavenEventStore
{
    private void HandleAppend(IDocumentSession session, Guid streamKey, List<Event> events)
    {
        CheckForNullOrEmptyEvents(events);

        var header = session
            .Include<StreamHeader>(x => x.AggregateId)
            .Include<StreamHeader>(x => x.HeadSeedId)
            .Load<StreamHeader>(StreamHeader.GetId(streamKey));

        CheckForMissingHeader(header, streamKey);

        AssignVersionToEvents(events, nextVersion: header.HeadPosition + 1);

        var existingAggregate = header.AggregateId is not null
            ? session.Load<Aggregate>(header.AggregateId)
            : null;

        CheckForMissingAggregate(existingAggregate, header.HeadStreamId, header.AggregateId);

        var existingSeed = header.HeadSeedId is not null
            ? session.Load<StreamSliceSeed>(header.HeadSeedId)
            : null;

        CheckForMissingSeed(existingSeed, header.HeadStreamId, header.HeadSeedId);

        if (existingAggregate is not null)
            ApplyNewEvents(existingAggregate, events);

        session.Advanced.Defer(BuildPatchCommandData(header.HeadStreamId, events));

        header.HeadPosition = events[^1].Version;

        AppendToGlobalLog(session, header.HeadStreamId, streamKey, events);
    }

    private async Task HandleAppendAsync(IAsyncDocumentSession session, Guid streamKey, List<Event> events, CancellationToken cancellationToken = default)
    {
        CheckForNullOrEmptyEvents(events);

        var header = await session
            .Include<StreamHeader>(x => x.AggregateId)
            .Include<StreamHeader>(x => x.HeadSeedId)
            .LoadAsync<StreamHeader>(StreamHeader.GetId(streamKey), cancellationToken);

        CheckForMissingHeader(header, streamKey);

        AssignVersionToEvents(events, nextVersion: header.HeadPosition + 1);

        var existingAggregate = header.AggregateId is not null
            ? await session.LoadAsync<Aggregate>(header.AggregateId, cancellationToken)
            : null;

        CheckForMissingAggregate(existingAggregate, header.HeadStreamId, header.AggregateId);

        var existingSeed = header.HeadSeedId is not null
            ? await session.LoadAsync<StreamSliceSeed>(header.HeadSeedId, cancellationToken)
            : null;

        CheckForMissingSeed(existingSeed, header.HeadStreamId, header.HeadSeedId);

        if (existingAggregate is not null)
            ApplyNewEvents(existingAggregate, events);

        session.Advanced.Defer(BuildPatchCommandData(header.HeadStreamId, events));

        header.HeadPosition = events[^1].Version;

        await AppendToGlobalLogAsync(session, header.HeadStreamId, streamKey, events, cancellationToken);
    }
    
    private static string SerializeEventsForPatch(List<Event> events) =>
        JsonConvert.SerializeObject(events, typeof(List<Event>),
            new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple
            });

    private static PatchCommandData BuildPatchCommandData(string headStreamId, List<Event> events) =>
        new(headStreamId, null, new ()
        {
            Script = "var e = JSON.parse(args.E); this.Events = this.Events.concat(e); this.UpdatedAt = args.T;",
            Values = { ["E"] = SerializeEventsForPatch(events), ["T"] = DateTime.UtcNow }
        });
}
