using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Raven.EventStore.Tests.Aggregates;
using Raven.EventStore.Tests.Events;
using Raven.EventStore.Tests.Streams;

namespace Raven.EventStore.Tests;

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
        
        Assert.That(stream, Is.Not.Null);
        Assert.That(stream.Id, Is.Not.Null);
        Assert.That(stream.StreamKey, Is.Not.EqualTo(Guid.Empty));
        Assert.That(stream.Position, Is.EqualTo(0));
        Assert.That(stream.AggregateId, Is.Null);
    }

    [Test]
    public async Task CreatesStream_WithSingleEvent()
    {
        var database = await CreateDatabase();
        
        var eventStore = InitEventStoreBuilder()
            .WithDatabaseName(database)
            .Build();

        var registered = UserRegisteredEvent.Create(username: "event-sorcerer", email: "lukasz@event-driven.com",
            role: "MEMBER");

        var stream = await eventStore.CreateStreamAndStoreAsync<UserStream>(registered);
        
        Assert.That(stream.Position, Is.EqualTo(1));
        Assert.That(stream.Events, Is.Not.Empty);
        Assert.That(stream.Events.Count, Is.EqualTo(1));
        Assert.That(stream.AggregateId, Is.Null);
        
        var @event = stream.Events[0];
        
        Assert.That(@event.EventId, Is.Not.EqualTo(Guid.Empty));
        Assert.That(@event.Timestamp, Is.Not.EqualTo(DateTime.MinValue));
        Assert.That(@event.Version, Is.EqualTo(1));
        Assert.That(@event, Is.InstanceOf<UserRegisteredEvent>());
        
        var typedEvent = (UserRegisteredEvent)@event;
        
        Assert.That(typedEvent.Username, Is.EqualTo("event-sorcerer"));
        Assert.That(typedEvent.Email, Is.EqualTo("lukasz@event-driven.com"));
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
        
        var registered = UserRegisteredEvent.Create(username: "event-sorcerer", email: "lukasz@event-driven.com",
            role: "MEMBER");
        
        var stream = await eventStore.CreateStreamAndStoreAsync<UserStream>(registered);

        var globalLog = await LoadSingleAsync<GlobalEventLog>(database);
        
        Assert.That(stream.Events, Has.Count.EqualTo(1));
        
        Assert.That(globalLog.StreamId, Is.EqualTo(stream.Id));
        Assert.That(globalLog.StreamKey, Is.EqualTo(stream.StreamKey));
        Assert.That(globalLog.Event, Is.InstanceOf<UserRegisteredEvent>());
        Assert.That(globalLog.Event.EventId, Is.EqualTo(stream.Events[0].EventId));
        Assert.That(globalLog.Sequence, Is.Not.Null);
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
        
        var registered = UserRegisteredEvent.Create(username: "event-sorcerer", email: "lukasz@event-driven.com",
            role: "MEMBER");
        
        var stream = await eventStore.CreateStreamAndStoreAsync<UserStream>(registered);

        var globalLog = await LoadSingleAsync<GlobalEventLog>(database);
        var aggregate = await LoadSingleAsync<User>(database);
        
        Assert.That(globalLog.StreamKey, Is.EqualTo(stream.StreamKey));
        Assert.That(aggregate.StreamKey, Is.EqualTo(stream.StreamKey));
        Assert.That(aggregate.Id, Is.EqualTo(stream.AggregateId));

        Assert.That(aggregate.Username, Is.EqualTo("event-sorcerer"));
        Assert.That(aggregate.Email, Is.EqualTo("lukasz@event-driven.com"));
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
        
        var registered = UserRegisteredEvent.Create(username: "event-sorcerer", email: "lukasz@event-driven.com",
            role: "MEMBER");
        
        var verified = UserVerifiedEvent.Create;
        
        var stream = await eventStore.CreateStreamAndStoreAsync<UserStream>(registered, verified);
        
        Assert.That(stream.Events, Has.Count.EqualTo(2));
        Assert.That(stream.Position, Is.EqualTo(2));
        Assert.That(stream.AggregateId, Is.Null);
        
        var event1 = stream.Events[0];
        var event2 = stream.Events[1];
        
        Assert.That(event1.Version, Is.EqualTo(1));
        Assert.That(event2.Version, Is.EqualTo(2));
        
        var globalLogs = await LoadAllAsync<GlobalEventLog>(database);
        
        Assert.That(globalLogs, Has.Count.EqualTo(2));
        
        var globalLog1 = globalLogs[0];
        var globalLog2 = globalLogs[1];
        
        Assert.That(globalLog1.Event, Is.InstanceOf<UserRegisteredEvent>());
        Assert.That(globalLog2.Event, Is.InstanceOf<UserVerifiedEvent>());
        Assert.That(globalLog1.Sequence, Is.LessThan(globalLog2.Sequence));
        Assert.That(globalLog1.StreamId, Is.EqualTo(stream.Id));
        Assert.That(globalLog2.StreamId, Is.EqualTo(stream.Id));
        Assert.That(stream.StreamKey, Is.EqualTo(globalLog1.StreamKey));
        Assert.That(stream.StreamKey, Is.EqualTo(globalLog2.StreamKey));
        Assert.That(globalLog1.StreamKey, Is.EqualTo(globalLog2.StreamKey));
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
        
        var registered = UserRegisteredEvent.Create(username: "event-sorcerer", email: "lukasz@event-driven.com",
            role: "MEMBER");
        var verified = UserVerifiedEvent.Create;
        var activated = UserActivatedEvent.Create;
        
        var stream = await eventStore.CreateStreamAndStoreAsync<UserStream>(registered, verified, activated);
        
        Assert.That(stream.Events, Has.Count.EqualTo(3));
        Assert.That(stream.Position, Is.EqualTo(3));
         
        var event1 = stream.Events[0];
        var event2 = stream.Events[1];
        var event3 = stream.Events[2];
        
        Assert.That(event1.Version, Is.EqualTo(1));
        Assert.That(event2.Version, Is.EqualTo(2));
        Assert.That(event3.Version, Is.EqualTo(3));
        
        var aggregate = await LoadSingleAsync<User>(database);
        
        Assert.That(aggregate.Id, Is.EqualTo(stream.AggregateId));
        Assert.That(aggregate.Username, Is.EqualTo("event-sorcerer"));
        Assert.That(aggregate.Email, Is.EqualTo("lukasz@event-driven.com"));
        Assert.That(aggregate.Role, Is.EqualTo("MEMBER"));
        Assert.That(aggregate.Status, Is.EqualTo("ACTIVATED"));
        
        var globalLogs = await LoadAllAsync<GlobalEventLog>(database);
        
        Assert.That(globalLogs, Has.Count.EqualTo(3));
        
        var globalLog1 = globalLogs[0];
        var globalLog2 = globalLogs[1];
        var globalLog3 = globalLogs[2];
        
        Assert.That(globalLog1.Event, Is.InstanceOf<UserRegisteredEvent>());
        Assert.That(globalLog2.Event, Is.InstanceOf<UserVerifiedEvent>());
        Assert.That(globalLog3.Event, Is.InstanceOf<UserActivatedEvent>());
        Assert.That(globalLog1.Sequence, Is.LessThan(globalLog2.Sequence));
        Assert.That(globalLog2.Sequence, Is.LessThan(globalLog3.Sequence));
        Assert.That(globalLog1.StreamId, Is.EqualTo(stream.Id));
        Assert.That(globalLog2.StreamId, Is.EqualTo(stream.Id));
        Assert.That(globalLog3.StreamId, Is.EqualTo(stream.Id));
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
        var event1 = UserRegisteredEvent.Create("event-sorcerer", "lukasz@event-driven.com", "MEMBER");
        Event event2 = null;
        
        var database = await CreateDatabase();
        
        var eventStore = InitEventStoreBuilder()
            .WithDatabaseName(database)
            .Build();
        
        var exception = Assert.ThrowsAsync<ArgumentException>(() => eventStore.CreateStreamAndStoreAsync<UserStream>(event1, event2));
        
        Assert.That(exception.Message, Does.Contain("at index 1"));
    }
}