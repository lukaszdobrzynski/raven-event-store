using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Raven.EventStore.Tests.Aggregates;
using Raven.EventStore.Tests.Asserts;
using Raven.EventStore.Tests.Events;
using Raven.EventStore.Tests.Streams;

namespace Raven.EventStore.Tests;

[Parallelizable]
public class CreateStreamTests : TestBase
{
    [Test]
    public async Task CreatesStream_WhenNoEvents()
    {
        var database = await CreateDatabase();
        var eventStore = InitEventStoreBuilder()
            .WithDatabaseName(database)
            .Build();

        var stream = await eventStore.CreateStreamAndStoreAsync<UserStream>();
        
        StreamAssert.NotNull(stream);
        StreamAssert.KeyNotEmpty(stream);
        StreamAssert.Position(stream, 0);
        
        StreamAssert.IdNotNull(stream);
        StreamAssert.AggregateIdNull(stream);
        
        StreamAssert.ArchiveNull(stream);
        StreamAssert.SeedNull(stream);
    }

    [Test]
    public async Task CreatesStream_WithSingleEvent()
    {
        var database = await CreateDatabase();
        var eventStore = InitEventStoreBuilder()
            .WithDatabaseName(database)
            .Build();

        var registered = UserRegisteredEvent.Create(username: "event-sorcerer", email: "john@event-sorcerer.com",
            role: "MEMBER");

        var stream = await eventStore.CreateStreamAndStoreAsync<UserStream>(registered);
        
        StreamAssert.Position(stream, 1);
        StreamAssert.EventsNotEmpty(stream);
        StreamAssert.EventsCount(stream, 1);
        StreamAssert.AggregateIdNull(stream);
        
        var @event = stream.Events[0];
        
        Assert.That(@event.EventId, Is.Not.EqualTo(Guid.Empty));
        Assert.That(@event.Timestamp, Is.Not.EqualTo(DateTime.MinValue));
        
        EventAssert.Version(@event, 1);
        EventAssert.Type<UserRegisteredEvent>(@event);
        
        var typedEvent = (UserRegisteredEvent)@event;
        
        Assert.That(typedEvent.Username, Is.EqualTo("event-sorcerer"));
        Assert.That(typedEvent.Email, Is.EqualTo("john@event-sorcerer.com"));
        Assert.That(typedEvent.Role, Is.EqualTo("MEMBER"));
    }

    [Test]
    public async Task CreatesStream_WithSingleEvent_AndGlobalLog()
    {
        var database = await CreateDatabase();
        var eventStore = InitEventStoreBuilder()
            .WithDatabaseName(database)
            .WithGlobalStreamLogging()
            .Build();
        
        var registered = UserRegisteredEvent.Create(username: "event-sorcerer", email: "john@event-sorcerer.com",
            role: "MEMBER");
        
        var stream = await eventStore.CreateStreamAndStoreAsync<UserStream>(registered);

        var globalLog = await LoadSingleAsync<GlobalEventLog>(database);
        
        StreamAssert.EventsCount(stream, 1);
        
        EventAssert.Type<UserRegisteredEvent>(globalLog.Event);
        
        GlobalLogAssert.StreamId(globalLog, stream.Id);
        GlobalLogAssert.StreamKey(globalLog, stream.StreamKey);
        GlobalLogAssert.EventId(globalLog, stream.Events[0].EventId);
        GlobalLogAssert.SequenceNotNull(globalLog);
    }

    [Test]
    public async Task CreatesStream_WithSingleEvent_GlobalLog_AndAggregate()
    {
        var database = await CreateDatabase();
        var eventStore = InitEventStoreBuilder()
            .WithDatabaseName(database)
            .WithAggregate(typeof(User))
            .WithGlobalStreamLogging()
            .Build();
        
        var registered = UserRegisteredEvent.Create(username: "event-sorcerer", email: "john@event-sorcerer.com",
            role: "MEMBER");
        
        var stream = await eventStore.CreateStreamAndStoreAsync<UserStream>(registered);

        var globalLog = await LoadSingleAsync<GlobalEventLog>(database);
        var aggregate = await LoadSingleAsync<User>(database);
        
        GlobalLogAssert.StreamKey(globalLog, stream.StreamKey);
        
        AggregateAssert.StreamKey(aggregate, stream.StreamKey);
        AggregateAssert.AggregateId(aggregate, stream.AggregateId);

        Assert.That(aggregate.Username, Is.EqualTo("event-sorcerer"));
        Assert.That(aggregate.Email, Is.EqualTo("john@event-sorcerer.com"));
        Assert.That(aggregate.Role, Is.EqualTo("MEMBER"));
    }

    [Test]
    public async Task CreatesStream_WithMultipleEvents_AndGlobalLog()
    {
        var database = await CreateDatabase();
        var eventStore = InitEventStoreBuilder()
            .WithDatabaseName(database)
            .WithGlobalStreamLogging()
            .Build();
        
        var registered = UserRegisteredEvent.Create(username: "event-sorcerer", email: "john@event-sorcerer.com",
            role: "MEMBER");
        
        var verified = UserVerifiedEvent.Create;
        
        var stream = await eventStore.CreateStreamAndStoreAsync<UserStream>(registered, verified);
        
        StreamAssert.EventsCount(stream, 2);
        StreamAssert.Position(stream, 2);
        StreamAssert.AggregateIdNull(stream);
        
        var event1 = stream.Events[0];
        var event2 = stream.Events[1];
        
        EventAssert.Version(event1, 1);
        EventAssert.Version(event2, 2);
        
        var globalLogs = await LoadAllAsync<GlobalEventLog>(database);
        
        GlobalLogAssert.LogCount(globalLogs, 2);
        
        var globalLog1 = globalLogs[0];
        var globalLog2 = globalLogs[1];
        
        EventAssert.Type<UserRegisteredEvent>(globalLog1.Event);
        EventAssert.Type<UserVerifiedEvent>(globalLog2.Event);
        
        GlobalLogAssert.SequenceLessThen(globalLog1, globalLog2.Sequence);
        GlobalLogAssert.StreamId(globalLog1, stream.Id);
        GlobalLogAssert.StreamId(globalLog2, stream.Id);
        GlobalLogAssert.StreamKey(globalLog1, stream.StreamKey);
        GlobalLogAssert.StreamKey(globalLog2, stream.StreamKey);
    }

    [Test]
    public async Task CreatesStream_WithMultipleEvents_Aggregate_AndGlobalLog()
    {
        var database = await CreateDatabase();
        var eventStore = InitEventStoreBuilder()
            .WithDatabaseName(database)
            .WithAggregate(typeof(User))
            .WithGlobalStreamLogging()
            .Build();
        
        var registered = UserRegisteredEvent.Create(username: "event-sorcerer", email: "john@event-sorcerer.com",
            role: "MEMBER");
        var verified = UserVerifiedEvent.Create;
        var activated = UserActivatedEvent.Create;
        
        var stream = await eventStore.CreateStreamAndStoreAsync<UserStream>(registered, verified, activated);
        
        StreamAssert.EventsCount(stream, 3);
        StreamAssert.Position(stream, 3);
         
        var event1 = stream.Events[0];
        var event2 = stream.Events[1];
        var event3 = stream.Events[2];
        
        EventAssert.Version(event1, 1);
        EventAssert.Version(event2, 2);
        EventAssert.Version(event3, 3);
        
        var aggregate = await LoadSingleAsync<User>(database);
        
        AggregateAssert.AggregateId(aggregate, stream.AggregateId);
        
        Assert.That(aggregate.Username, Is.EqualTo("event-sorcerer"));
        Assert.That(aggregate.Email, Is.EqualTo("john@event-sorcerer.com"));
        Assert.That(aggregate.Role, Is.EqualTo("MEMBER"));
        Assert.That(aggregate.Status, Is.EqualTo("ACTIVATED"));
        
        var globalLogs = await LoadAllAsync<GlobalEventLog>(database);
        
        GlobalLogAssert.LogCount(globalLogs, 3);
        
        var globalLog1 = globalLogs[0];
        var globalLog2 = globalLogs[1];
        var globalLog3 = globalLogs[2];
        
        EventAssert.Type<UserRegisteredEvent>(globalLog1.Event);
        EventAssert.Type<UserVerifiedEvent>(globalLog2.Event);
        EventAssert.Type<UserActivatedEvent>(globalLog3.Event);
        
        GlobalLogAssert.SequenceLessThen(globalLog1, globalLog2.Sequence);
        GlobalLogAssert.SequenceLessThen(globalLog2, globalLog3.Sequence);
        GlobalLogAssert.StreamId(globalLog1, stream.Id);
        GlobalLogAssert.StreamId(globalLog2, stream.Id);
        GlobalLogAssert.StreamId(globalLog3, stream.Id);
    }

    [Test]
    public async Task CreatesStream_WithProvidedId()
    {
        var database = await CreateDatabase();
        var eventStore = InitEventStoreBuilder()
            .WithDatabaseName(database)
            .Build();
        
        var id = await CreateSemanticId<UserStream>(database, idSuffix:"2025-05");
        
        var stream = await eventStore.CreateStreamAndStoreAsync<UserStream>(id);
        
        Assert.That(stream.Id, Is.EqualTo(id));
        Assert.That(stream.Id, Does.EndWith("2025-05"));
    }

    [Test]
    public async Task Throws_When_EventsAreNull()
    {
        List<Event> events = null;
        
        var database = await CreateDatabase();
        var eventStore = InitEventStoreBuilder()
            .WithDatabaseName(database)
            .Build();
        
        Assert.ThrowsAsync<ArgumentException>(async () => await eventStore.CreateStreamAndStoreAsync<UserStream>(events));
    }

    [Test]
    public async Task Throws_WhenEventList_ContainsNull()
    {
        var event1 = UserRegisteredEvent.Create("event-sorcerer", "john@event-sorcerer.com", "MEMBER");
        Event event2 = null;
        
        var database = await CreateDatabase();
        var eventStore = InitEventStoreBuilder()
            .WithDatabaseName(database)
            .Build();
        
        var exception = Assert.ThrowsAsync<ArgumentException>(async () => await eventStore.CreateStreamAndStoreAsync<UserStream>(event1, event2));
        
        Assert.That(exception.Message, Does.Contain("Null event found at index 1"));
    }
}