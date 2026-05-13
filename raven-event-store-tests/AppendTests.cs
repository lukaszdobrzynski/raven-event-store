using System;
using System.Threading.Tasks;
using Raven.EventStore.Exceptions;
using Raven.EventStore.Tests.Asserts;
using RavenEventStoreTestModels.Aggregates;
using RavenEventStoreTestModels.Events;
using RavenEventStoreTestModels.Streams;

namespace Raven.EventStore.Tests;

[Parallelizable]
public class AppendTests : TestBase
{
    [Test]
    public async Task Throws_WhenStreamDoesNotExist()
    {
        var streamKey = Guid.NewGuid();

        var databaseName = await CreateDatabase();

        var eventStore = InitEventStoreBuilder(databaseName)
            .Build();

        var changedEmail = UserChangedEmailEvent.Create("john@event-sorcerer.io");

        Assert.ThrowsAsync<NonExistentHeaderException>(async () => await eventStore.AppendAndStoreAsync(streamKey, changedEmail));
    }

    [Test]
    public async Task Throws_WhenEventsAreNull()
    {
        Event @event = null;

        var databaseName = await CreateDatabase();
        var eventStore = InitEventStoreBuilder(databaseName)
            .Build();

        var registered = UserRegisteredEvent.Create("event-sorcerer", "john@event-sorcerer.com", "MEMBER");
        var stream = eventStore.CreateStreamAndStore<UserStream>(registered);

        Assert.ThrowsAsync<ArgumentException>(async () => await eventStore.AppendAndStoreAsync(stream.StreamKey, @event));
    }

    [Test]
    public async Task Throws_WhenEvents_ContainNull()
    {
        var event1 = UserVerifiedEvent.Create;
        Event event2 = null;

        var databaseName = await CreateDatabase();
        var eventStore = InitEventStoreBuilder(databaseName)
            .Build();

        var registered = UserRegisteredEvent.Create("event-sorcerer", "john@event-sorcerer.com", "MEMBER");
        var stream = eventStore.CreateStreamAndStore<UserStream>(registered);

        var exception = Assert.ThrowsAsync<ArgumentException>(async () => await eventStore.AppendAndStoreAsync(stream.StreamKey, event1, event2));

        Assert.That(exception.Message, Does.Contain("Null event found at index 1"));
    }

    [Test]
    public async Task Throws_WhenStreamReferencesAggregate_But_AggregateDoesNotExist()
    {
        var databaseName = await CreateDatabase();
        var eventStore = InitEventStoreBuilder(databaseName)
            .WithAggregate(typeof(User))
            .Build();

        var stream = eventStore.CreateStreamAndStore<UserStream>(
            UserRegisteredEvent.Create("event-sorcerer", "john@event-sorcerer.com", "MEMBER"));

        await AssertDocumentExistsInDb<User>(databaseName, stream.AggregateId);

        await Delete(databaseName, stream.AggregateId);

        Assert.ThrowsAsync<NonExistentAggregateException>(async () => await eventStore.AppendAndStoreAsync(stream.StreamKey, UserActivatedEvent.Create));
    }

    [Test]
    public async Task Throws_WhenStreamReferencesSliceSeed_But_SliceSeedDoesNotExist()
    {
        var databaseName = await CreateDatabase();
        var eventStore = InitEventStoreBuilder(databaseName)
            .WithAggregate(typeof(User))
            .Build();

        var sourceStream = eventStore.CreateStreamAndStore<UserStream>(
            UserRegisteredEvent.Create("event-sorcerer", "john@event-sorcerer.com", "MEMBER"));

        var nextStreamId = CreateSliceStreamNextId(sourceStream.Id, "2025-05");

        var streamSlice = eventStore.SliceStreamAndStore<UserStream>(sourceStream.StreamKey, nextStreamId, UserVerifiedEvent.Create);

        await AssertDocumentExistsInDb<StreamSliceSeed>(databaseName, streamSlice.SeedId);

        await Delete(databaseName, streamSlice.SeedId);

        Assert.ThrowsAsync<NonExistentSeedException>(async () => await eventStore.AppendAndStoreAsync(streamSlice.StreamKey, UserActivatedEvent.Create));
    }

    [Test]
    public async Task Throws_WhenStreamReferencesSliceSeed_But_SliceSeedStateIsNull()
    {
        var databaseName = await CreateDatabase();
        var eventStore = InitEventStoreBuilder(databaseName)
            .WithAggregate(typeof(User))
            .Build();

        var sourceStream = eventStore.CreateStreamAndStore<UserStream>(
            UserRegisteredEvent.Create("event-sorcerer", "john@event-sorcerer.com", "MEMBER"));

        var nextStreamId = CreateSliceStreamNextId(sourceStream.Id, "2025-05");
        var streamSlice = eventStore.SliceStreamAndStore<UserStream>(sourceStream.StreamKey, nextStreamId, UserVerifiedEvent.Create);

        await AssertDocumentExistsInDb<StreamSliceSeed>(databaseName, streamSlice.SeedId);

        var seed = await LoadAsync<StreamSliceSeed>(databaseName, streamSlice.SeedId);
        seed.State = null;
        await Store(databaseName, seed);

        var exception = Assert.ThrowsAsync<NonExistentSeedException>(async () =>
            await eventStore.AppendAndStoreAsync(streamSlice.StreamKey, UserActivatedEvent.Create));

        Assert.That(exception.Message, Does.Contain("the seed document exists but contains no state snapshot"));
    }

    [Test]
    public async Task StreamHeader_HeadPosition_AdvancesAfterAppend()
    {
        var databaseName = await CreateDatabase();
        var eventStore = InitEventStoreBuilder(databaseName)
            .Build();

        var stream = eventStore.CreateStreamAndStore<UserStream>(
            UserRegisteredEvent.Create("event-sorcerer", "john@event-sorcerer.com", "MEMBER"));

        await eventStore.AppendAndStoreAsync(stream.StreamKey, UserVerifiedEvent.Create, UserActivatedEvent.Create);

        var header = await LoadSingleAsync<StreamHeader>(databaseName);

        StreamHeaderAssert.HeadStreamId(header, stream.Id);
        StreamHeaderAssert.HeadPosition(header, 3);
        StreamHeaderAssert.HeadFirstVersion(header, 1);
        StreamHeaderAssert.HeadFirstTimestamp(header, stream.Events[0].Timestamp);
    }

    [Test]
    public async Task AppendsSingleEventToStream()
    {
        var databaseName = await CreateDatabase();
        var eventStore = InitEventStoreBuilder(databaseName)
            .Build();

        var registered = UserRegisteredEvent.Create("event-sorcerer", "john@event-sorcerer.com", "MEMBER");
        var stream = eventStore.CreateStreamAndStore<UserStream>(registered);

        await eventStore.AppendAndStoreAsync(stream.StreamKey, UserChangedEmailEvent.Create("alice@event-sorcerer.io"));

        var streamFromDb = await LoadSingleAsync<UserStream>(databaseName);

        StreamAssert.Position(streamFromDb, 2);
        StreamAssert.ArchiveNull(streamFromDb);
        StreamAssert.SeedNull(streamFromDb);
        StreamAssert.EventsCount(streamFromDb, 2);

        EventAssert.Version(streamFromDb.Events[1], 2);
        EventAssert.Type<UserChangedEmailEvent>(streamFromDb.Events[1]);

        Assert.That(streamFromDb.Events[1].Timestamp, Is.Not.EqualTo(DateTime.MinValue));
        Assert.That(streamFromDb.Events[1].EventId, Is.Not.EqualTo(Guid.Empty));
    }

    [Test]
    public async Task AppendsSingleEventToStream_WithGlobalLog()
    {
        var databaseName = await CreateDatabase();
        var eventStore = InitEventStoreBuilder(databaseName)
            .WithGlobalStreamLogging()
            .Build();

        var registered = UserRegisteredEvent.Create("event-sorcerer", "john@event-sorcerer.com", "MEMBER");
        var stream = eventStore.CreateStreamAndStore<UserStream>(registered);

        await eventStore.AppendAndStoreAsync(stream.StreamKey, UserChangedEmailEvent.Create("alice@event-sorcerer.io"));

        var streamFromDb = await LoadSingleAsync<UserStream>(databaseName);
        var globalLogs = await LoadAllAsync<GlobalEventLog>(databaseName);

        StreamAssert.EventsCount(streamFromDb, 2);
        StreamAssert.Position(streamFromDb, 2);

        GlobalLogAssert.LogCount(globalLogs, 2);

        var appendedEvent = streamFromDb.Events[1];
        var appendedGlobalLog = globalLogs[1];

        EventAssert.Version(appendedEvent, 2);
        EventAssert.Type<UserChangedEmailEvent>(appendedGlobalLog.Event);

        GlobalLogAssert.StreamId(appendedGlobalLog, streamFromDb.Id);
        GlobalLogAssert.StreamKey(appendedGlobalLog, streamFromDb.StreamKey);
        GlobalLogAssert.EventId(appendedGlobalLog, streamFromDb.Events[1].EventId);
    }

    [Test]
    public async Task AppendsMultipleEvents_WithAggregate_AndGlobalLog()
    {
        var databaseName = await CreateDatabase();
        var eventStore = InitEventStoreBuilder(databaseName)
            .WithAggregate(typeof(User))
            .WithGlobalStreamLogging()
            .Build();

        var registered = UserRegisteredEvent.Create("event-sorcerer", "john@event-sorcerer.com", "MEMBER");
        var stream = eventStore.CreateStreamAndStore<UserStream>(registered);

        await eventStore.AppendAndStoreAsync(
            stream.StreamKey,
            UserVerifiedEvent.Create,
            UserActivatedEvent.Create,
            UserRoleChangedEvent.Create("ADMIN"));

        var streamFromDb = await LoadSingleAsync<UserStream>(databaseName);
        var aggregate = await LoadSingleAsync<User>(databaseName);
        var globalLogs = await LoadAllAsync<GlobalEventLog>(databaseName);

        Assert.That(aggregate.Username, Is.EqualTo("event-sorcerer"));
        Assert.That(aggregate.Email, Is.EqualTo("john@event-sorcerer.com"));
        Assert.That(aggregate.Role, Is.EqualTo("ADMIN"));
        Assert.That(aggregate.Status, Is.EqualTo("ACTIVATED"));

        StreamAssert.EventsCount(streamFromDb, 4);
        StreamAssert.Position(streamFromDb, 4);
        StreamAssert.ArchiveNull(streamFromDb);
        StreamAssert.SeedNull(streamFromDb);
        StreamAssert.AggregateIdNotNull(streamFromDb);

        AggregateAssert.StreamKey(aggregate, streamFromDb.StreamKey);
        AggregateAssert.AggregateId(aggregate, streamFromDb.AggregateId);

        GlobalLogAssert.LogCount(globalLogs, 4);

        var log1 = globalLogs[0];
        var log2 = globalLogs[1];
        var log3 = globalLogs[2];
        var log4 = globalLogs[3];

        GlobalLogAssert.StreamId(log1, streamFromDb.Id);
        GlobalLogAssert.StreamId(log2, streamFromDb.Id);
        GlobalLogAssert.StreamId(log3, streamFromDb.Id);
        GlobalLogAssert.StreamId(log4, streamFromDb.Id);

        GlobalLogAssert.EventId(log1, streamFromDb.Events[0].EventId);
        GlobalLogAssert.EventId(log2, streamFromDb.Events[1].EventId);
        GlobalLogAssert.EventId(log3, streamFromDb.Events[2].EventId);
        GlobalLogAssert.EventId(log4, streamFromDb.Events[3].EventId);

        EventAssert.Type<UserRegisteredEvent>(log1.Event);
        EventAssert.Type<UserVerifiedEvent>(log2.Event);
        EventAssert.Type<UserActivatedEvent>(log3.Event);
        EventAssert.Type<UserRoleChangedEvent>(log4.Event);

        EventAssert.Version(log1.Event, 1);
        EventAssert.Version(log2.Event, 2);
        EventAssert.Version(log3.Event, 3);
        EventAssert.Version(log4.Event, 4);

        EventAssert.Version(streamFromDb.Events[0], 1);
        EventAssert.Version(streamFromDb.Events[1], 2);
        EventAssert.Version(streamFromDb.Events[2], 3);
        EventAssert.Version(streamFromDb.Events[3], 4);
    }
}
