using System;
using System.Threading.Tasks;
using Raven.EventStore.Exceptions;
using Raven.EventStore.Tests.Aggregates;
using Raven.EventStore.Tests.Asserts;
using Raven.EventStore.Tests.Events;
using Raven.EventStore.Tests.Streams;

namespace Raven.EventStore.Tests;

[Parallelizable]
public class AppendTests : TestBase
{
    [Test]
    public async Task Throws_WhenStreamDoesNotExist()
    {
        const string streamId = "DOES-NOT-EXIST";
        
        var database = await CreateDatabase();

        await AssertNoDocumentInDb<UserStream>(database, streamId);
        
        var eventStore = InitEventStoreBuilder()
            .WithDatabaseName(database)
            .Build();

        var changedEmail = UserChangedEmailEvent.Create("john@event-sorcerer.io");
        
        Assert.ThrowsAsync<NonExistentStreamException>(async () => await eventStore.AppendAndStoreAsync<UserStream>(streamId, changedEmail));
    }

    [Test]
    public async Task Throws_WhenEventsAreNull()
    {
        Event @event = null;
        
        var database = await CreateDatabase();
        var eventStore = InitEventStoreBuilder()
            .WithDatabaseName(database)
            .Build();
        
        var registered = UserRegisteredEvent.Create("event-sorcerer", "john@event-sorcerer.com", "MEMBER");
        var stream = eventStore.CreateStreamAndStore<UserStream>(registered);

        Assert.ThrowsAsync<ArgumentException>(async() => await eventStore.AppendAndStoreAsync<UserStream>(stream.Id, @event));
    }

    [Test]
    public async Task Throws_WhenEvents_ContainNull()
    {
        var event1 = UserVerifiedEvent.Create;
        Event event2 = null;
        
        var database = await CreateDatabase();
        var eventStore = InitEventStoreBuilder()
            .WithDatabaseName(database)
            .Build();
        
        var registered = UserRegisteredEvent.Create("event-sorcerer", "john@event-sorcerer.com", "MEMBER");
        var stream = eventStore.CreateStreamAndStore<UserStream>(registered);
        
        var exception = Assert.ThrowsAsync<ArgumentException>(async () => await eventStore.AppendAndStoreAsync<UserStream>(stream.Id, event1, event2));
        
        Assert.That(exception.Message, Does.Contain("Null event found at index 1"));
    }

    [Test]
    public async Task Throws_WhenAppends_ToNonHead()
    {
        var database = await CreateDatabase();
        var eventStore = InitEventStoreBuilder()
            .WithDatabaseName(database)
            .Build();

        var sourceStreamId = await CreateSemanticId<UserStream>(database, "2025-04");
        var sourceStream = eventStore.CreateStreamAndStore<UserStream>(sourceStreamId, 
            UserRegisteredEvent.Create("event-sorcerer", "john@event-sorcerer.com", "MEMBER"));
        
        var sliceStreamId = CreateSliceStreamNextId(sourceStreamId, "2025-05");
        var sliceStream = eventStore.SliceStreamAndStore<UserStream>(sourceStreamId, sliceStreamId, UserVerifiedEvent.Create);
        
        var sourceStreamFromDbBefore = await LoadAsync<UserStream>(database, sourceStreamId);
        
        StreamAssert.IsNotHead(sourceStreamFromDbBefore);
        StreamAssert.IsHead(sliceStream);
        StreamAssert.Position(sourceStreamFromDbBefore, 1);
        StreamAssert.Position(sliceStream, 2);
        StreamAssert.EventsCount(sourceStreamFromDbBefore, 1);
        StreamAssert.EventsCount(sliceStream, 1);
        StreamAssert.PreviousSliceId(sourceStreamFromDbBefore, null);
        StreamAssert.PreviousSliceId(sliceStream, sourceStreamFromDbBefore.Id);
        StreamAssert.NextSliceId(sourceStreamFromDbBefore, sliceStream.Id);
        StreamAssert.NextSliceId(sliceStream, null);

        Assert.ThrowsAsync<AppendToNotHeadException>(async () => await eventStore.AppendAndStoreAsync<UserStream>(
            sourceStreamId,
            UserActivatedEvent.Create));
        
        var sourceStreamFromDbAfter = await LoadAsync<UserStream>(database, sourceStreamId);
        var sliceStreamFromDbAfter = await LoadAsync<UserStream>(database, sliceStreamId);
        
        StreamAssert.IsNotHead(sourceStreamFromDbAfter);
        StreamAssert.IsHead(sliceStreamFromDbAfter);
        StreamAssert.Position(sourceStreamFromDbAfter, sourceStreamFromDbBefore.Position);
        StreamAssert.Position(sliceStreamFromDbAfter, sliceStream.Position);
        StreamAssert.EventsCount(sourceStreamFromDbAfter, sourceStreamFromDbBefore.Events.Count);
        StreamAssert.EventsCount(sliceStreamFromDbAfter, sliceStream.Events.Count);
        StreamAssert.PreviousSliceId(sourceStreamFromDbAfter, sourceStreamFromDbBefore.PreviousSliceId);
        StreamAssert.PreviousSliceId(sliceStreamFromDbAfter, sliceStream.PreviousSliceId);
        StreamAssert.NextSliceId(sourceStreamFromDbAfter, sourceStreamFromDbBefore.NextSliceId);
        StreamAssert.NextSliceId(sliceStreamFromDbAfter, sliceStream.NextSliceId);
    }

    [Test]
    public async Task AppendsSingleEventToStream()
    {
        var database = await CreateDatabase();
        var eventStore = InitEventStoreBuilder()
            .WithDatabaseName(database)
            .Build();
        
        var registered = UserRegisteredEvent.Create("event-sorcerer", "john@event-sorcerer.com", "MEMBER");
        var stream = eventStore.CreateStreamAndStore<UserStream>(registered);
        
        await eventStore.AppendAndStoreAsync<UserStream>(stream.Id, UserChangedEmailEvent.Create("alice@event-sorcerer.io"));
        
        var streamFromDb = await LoadSingleAsync<UserStream>(database);
        
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
        var database = await CreateDatabase();
        var eventStore = InitEventStoreBuilder()
            .WithDatabaseName(database)
            .WithGlobalStreamLogging()
            .Build();
        
        var registered = UserRegisteredEvent.Create("event-sorcerer", "john@event-sorcerer.com", "MEMBER");
        var stream = eventStore.CreateStreamAndStore<UserStream>(registered);
        
        await eventStore.AppendAndStoreAsync<UserStream>(stream.Id, UserChangedEmailEvent.Create("alice@event-sorcerer.io"));
        
        var streamFromDb = await LoadSingleAsync<UserStream>(database);
        var globalLogs = await LoadAllAsync<GlobalEventLog>(database);
        
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
        var database = await CreateDatabase();
        var eventStore = InitEventStoreBuilder()
            .WithDatabaseName(database)
            .WithAggregate(typeof(User))
            .WithGlobalStreamLogging()
            .Build();
        
        var registered = UserRegisteredEvent.Create("event-sorcerer", "john@event-sorcerer.com", "MEMBER");
        var stream = eventStore.CreateStreamAndStore<UserStream>(registered);
        
        await eventStore.AppendAndStoreAsync<UserStream>(
            stream.Id, 
            UserVerifiedEvent.Create, 
            UserActivatedEvent.Create, 
            UserRoleChangedEvent.Create("ADMIN"));
        
        var streamFromDb = await LoadSingleAsync<UserStream>(database);
        var aggregate = await LoadSingleAsync<User>(database);
        var globalLogs = await LoadAllAsync<GlobalEventLog>(database);
        
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
        
        GlobalLogAssert.SequenceLessThen(log1, log2.Sequence);
        GlobalLogAssert.SequenceLessThen(log2, log3.Sequence);
        GlobalLogAssert.SequenceLessThen(log3, log4.Sequence);
        
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