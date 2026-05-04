using System;
using System.Threading.Tasks;
using Raven.EventStore.Exceptions;
using Raven.EventStore.Tests.Aggregates;
using Raven.EventStore.Tests.Events;
using Raven.EventStore.Tests.Streams;

namespace Raven.EventStore.Tests;

[Parallelizable]
public class GetAggregateAtTests : TestBase
{
    [Test]
    public async Task ReturnsNull_WhenStreamDoesNotExist()
    {
        const string notExistsStreamId = "not-exists-stream-id";

        var databaseName = await CreateDatabase();
        var eventStore = InitEventStoreBuilder(databaseName)
            .WithAggregate(typeof(User))
            .Build();

        await AssertNoDocumentInDb<User>(databaseName, notExistsStreamId);

        var aggregate = await eventStore.GetAggregateAtAsync<User, UserStream>(notExistsStreamId, DateTime.UtcNow);

        Assert.That(aggregate, Is.Null);
    }

    [Test]
    public async Task ReturnsNull_WhenTimestampIsBeforeAllEvents_SingleStream()
    {
        var eventTimestamp = new DateTime(2025, 1, 2, 0, 0, 0, DateTimeKind.Utc);
        var queryTimestamp = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var databaseName = await CreateDatabase();
        var eventStore = InitEventStoreBuilder(databaseName)
            .WithAggregate(typeof(User))
            .Build();

        var @event = UserRegisteredEvent.Create("event-sorcerer", "john@event-sorcerer.com", "MEMBER");
        @event.Timestamp = eventTimestamp;

        var stream = await eventStore.CreateStreamAndStoreAsync<UserStream>(@event);

        var aggregate = await eventStore.GetAggregateAtAsync<User, UserStream>(stream.Id, queryTimestamp);

        Assert.That(aggregate, Is.Null);
    }

    [Test]
    public async Task GetsAggregate_AtTimestamp_AfterFirstEvent_SingleStreamWithThreeEvents()
    {
        var t1 = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var t2 = new DateTime(2025, 1, 2, 0, 0, 0, DateTimeKind.Utc);
        var t3 = new DateTime(2025, 1, 3, 0, 0, 0, DateTimeKind.Utc);
        var queryTimestamp = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        var databaseName = await CreateDatabase();
        var eventStore = InitEventStoreBuilder(databaseName)
            .WithAggregate(typeof(User))
            .Build();

        var registeredEvent = UserRegisteredEvent.Create("event-sorcerer", "john@event-sorcerer.com", "MEMBER");
        registeredEvent.Timestamp = t1;

        var verifiedEvent = UserVerifiedEvent.Create;
        verifiedEvent.Timestamp = t2;

        var activatedEvent = UserActivatedEvent.Create;
        activatedEvent.Timestamp = t3;

        var stream = await eventStore.CreateStreamAndStoreAsync<UserStream>(registeredEvent, verifiedEvent, activatedEvent);

        var aggregate = await eventStore.GetAggregateAtAsync<User, UserStream>(stream.Id, queryTimestamp);

        Assert.That(aggregate.Username, Is.EqualTo("event-sorcerer"));
        Assert.That(aggregate.Email, Is.EqualTo("john@event-sorcerer.com"));
        Assert.That(aggregate.Status, Is.EqualTo("REGISTERED"));
        Assert.That(aggregate.Role, Is.EqualTo("MEMBER"));
    }

    [Test]
    public async Task GetsAggregate_AtTimestamp_AfterSecondEvent_SingleStreamWithThreeEvents()
    {
        var t1 = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var t2 = new DateTime(2025, 1, 2, 0, 0, 0, DateTimeKind.Utc);
        var t3 = new DateTime(2025, 1, 3, 0, 0, 0, DateTimeKind.Utc);
        var queryTimestamp = new DateTime(2025, 1, 2, 12, 0, 0, DateTimeKind.Utc);

        var databaseName = await CreateDatabase();
        var eventStore = InitEventStoreBuilder(databaseName)
            .WithAggregate(typeof(User))
            .Build();

        var registeredEvent = UserRegisteredEvent.Create("event-sorcerer", "john@event-sorcerer.com", "MEMBER");
        registeredEvent.Timestamp = t1;

        var verifiedEvent = UserVerifiedEvent.Create;
        verifiedEvent.Timestamp = t2;

        var activatedEvent = UserActivatedEvent.Create;
        activatedEvent.Timestamp = t3;

        var stream = await eventStore.CreateStreamAndStoreAsync<UserStream>(registeredEvent, verifiedEvent, activatedEvent);

        var aggregate = await eventStore.GetAggregateAtAsync<User, UserStream>(stream.Id, queryTimestamp);

        Assert.That(aggregate.Status, Is.EqualTo("VERIFIED"));
    }

    [Test]
    public async Task GetsAggregate_AtTimestamp_AfterAllEvents_SingleStreamWithThreeEvents()
    {
        var t1 = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var t2 = new DateTime(2025, 1, 2, 0, 0, 0, DateTimeKind.Utc);
        var t3 = new DateTime(2025, 1, 3, 0, 0, 0, DateTimeKind.Utc);
        var queryTimestamp = new DateTime(2025, 1, 4, 0, 0, 0, DateTimeKind.Utc);

        var databaseName = await CreateDatabase();
        var eventStore = InitEventStoreBuilder(databaseName)
            .WithAggregate(typeof(User))
            .Build();

        var registeredEvent = UserRegisteredEvent.Create("event-sorcerer", "john@event-sorcerer.com", "MEMBER");
        registeredEvent.Timestamp = t1;

        var verifiedEvent = UserVerifiedEvent.Create;
        verifiedEvent.Timestamp = t2;

        var activatedEvent = UserActivatedEvent.Create;
        activatedEvent.Timestamp = t3;

        var stream = await eventStore.CreateStreamAndStoreAsync<UserStream>(registeredEvent, verifiedEvent, activatedEvent);

        var aggregate = await eventStore.GetAggregateAtAsync<User, UserStream>(stream.Id, queryTimestamp);

        Assert.That(aggregate.Status, Is.EqualTo("ACTIVATED"));
    }

    [Test]
    public async Task GetsAggregate_AtTimestamp_InCurrentSlice()
    {
        var t1 = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var t2 = new DateTime(2025, 1, 2, 0, 0, 0, DateTimeKind.Utc);
        var t3 = new DateTime(2025, 1, 3, 0, 0, 0, DateTimeKind.Utc);
        var t4 = new DateTime(2025, 1, 4, 0, 0, 0, DateTimeKind.Utc);
        var queryTimestamp = new DateTime(2025, 1, 3, 12, 0, 0, DateTimeKind.Utc);

        var databaseName = await CreateDatabase();
        var eventStore = InitEventStoreBuilder(databaseName)
            .WithAggregate(typeof(User))
            .Build();

        var registeredEvent = UserRegisteredEvent.Create("event-sorcerer", "john@event-sorcerer.com", "MEMBER");
        registeredEvent.Timestamp = t1;

        var verifiedEvent = UserVerifiedEvent.Create;
        verifiedEvent.Timestamp = t2;

        var activatedEvent = UserActivatedEvent.Create;
        activatedEvent.Timestamp = t3;

        var changedEmailEvent = UserChangedEmailEvent.Create("john@event-sorcerer.io");
        changedEmailEvent.Timestamp = t4;

        var sourceStream = await eventStore.CreateStreamAndStoreAsync<UserStream>(registeredEvent, verifiedEvent);
        var headStream = await eventStore.SliceStreamAndStoreAsync<UserStream>(sourceStream.Id, activatedEvent, changedEmailEvent);

        var aggregate = await eventStore.GetAggregateAtAsync<User, UserStream>(headStream.Id, queryTimestamp);

        Assert.That(aggregate.Status, Is.EqualTo("ACTIVATED"));
        Assert.That(aggregate.Email, Is.EqualTo("john@event-sorcerer.com"));
    }

    [Test]
    public async Task GetsAggregate_AtTimestamp_InPreviousSlice()
    {
        var t1 = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var t2 = new DateTime(2025, 1, 2, 0, 0, 0, DateTimeKind.Utc);
        var t3 = new DateTime(2025, 1, 3, 0, 0, 0, DateTimeKind.Utc);
        var t4 = new DateTime(2025, 1, 4, 0, 0, 0, DateTimeKind.Utc);
        var t5 = new DateTime(2025, 1, 5, 0, 0, 0, DateTimeKind.Utc);
        var queryTimestamp = new DateTime(2025, 1, 3, 12, 0, 0, DateTimeKind.Utc);

        var databaseName = await CreateDatabase();
        var eventStore = InitEventStoreBuilder(databaseName)
            .WithAggregate(typeof(User))
            .Build();

        var registeredEvent = UserRegisteredEvent.Create("event-sorcerer", "john@event-sorcerer.com", "MEMBER");
        registeredEvent.Timestamp = t1;

        var verifiedEvent = UserVerifiedEvent.Create;
        verifiedEvent.Timestamp = t2;

        var activatedEvent = UserActivatedEvent.Create;
        activatedEvent.Timestamp = t3;

        var changedEmailEvent = UserChangedEmailEvent.Create("john@event-sorcerer.io");
        changedEmailEvent.Timestamp = t4;

        var roleChangedEvent = UserRoleChangedEvent.Create("ADMIN");
        roleChangedEvent.Timestamp = t5;

        var slice1 = await eventStore.CreateStreamAndStoreAsync<UserStream>(registeredEvent, verifiedEvent);
        var slice2 = await eventStore.SliceStreamAndStoreAsync<UserStream>(slice1.Id, activatedEvent, changedEmailEvent);
        var headStream = await eventStore.SliceStreamAndStoreAsync<UserStream>(slice2.Id, roleChangedEvent);

        var aggregate = await eventStore.GetAggregateAtAsync<User, UserStream>(headStream.Id, queryTimestamp);

        Assert.That(aggregate.Status, Is.EqualTo("ACTIVATED"));
        Assert.That(aggregate.Email, Is.EqualTo("john@event-sorcerer.com"));
    }

    [Test]
    public async Task GetsAggregate_AtTimestamp_InEarliestSlice()
    {
        var t1 = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var t2 = new DateTime(2025, 1, 2, 0, 0, 0, DateTimeKind.Utc);
        var t3 = new DateTime(2025, 1, 3, 0, 0, 0, DateTimeKind.Utc);
        var queryTimestamp = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        var databaseName = await CreateDatabase();
        var eventStore = InitEventStoreBuilder(databaseName)
            .WithAggregate(typeof(User))
            .Build();

        var registeredEvent = UserRegisteredEvent.Create("event-sorcerer", "john@event-sorcerer.com", "MEMBER");
        registeredEvent.Timestamp = t1;

        var verifiedEvent = UserVerifiedEvent.Create;
        verifiedEvent.Timestamp = t2;

        var activatedEvent = UserActivatedEvent.Create;
        activatedEvent.Timestamp = t3;

        var slice1 = await eventStore.CreateStreamAndStoreAsync<UserStream>(registeredEvent, verifiedEvent);
        var headStream = await eventStore.SliceStreamAndStoreAsync<UserStream>(slice1.Id, activatedEvent);

        var aggregate = await eventStore.GetAggregateAtAsync<User, UserStream>(headStream.Id, queryTimestamp);

        Assert.That(aggregate.Username, Is.EqualTo("event-sorcerer"));
        Assert.That(aggregate.Status, Is.EqualTo("REGISTERED"));
    }

    [Test]
    public async Task Throws_WhenSeedIsMissing()
    {
        var t1 = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var t2 = new DateTime(2025, 1, 2, 0, 0, 0, DateTimeKind.Utc);
        var t3 = new DateTime(2025, 1, 3, 0, 0, 0, DateTimeKind.Utc);
        var queryTimestamp = new DateTime(2025, 1, 4, 0, 0, 0, DateTimeKind.Utc);

        var databaseName = await CreateDatabase();
        var eventStore = InitEventStoreBuilder(databaseName)
            .WithAggregate(typeof(User))
            .Build();

        var registeredEvent = UserRegisteredEvent.Create("event-sorcerer", "john@event-sorcerer.com", "MEMBER");
        registeredEvent.Timestamp = t1;

        var verifiedEvent = UserVerifiedEvent.Create;
        verifiedEvent.Timestamp = t2;

        var activatedEvent = UserActivatedEvent.Create;
        activatedEvent.Timestamp = t3;

        var slice1 = await eventStore.CreateStreamAndStoreAsync<UserStream>(registeredEvent, verifiedEvent);
        var headStream = await eventStore.SliceStreamAndStoreAsync<UserStream>(slice1.Id, activatedEvent);

        await Delete(databaseName, headStream.SeedId);

        Assert.ThrowsAsync<NonExistentSeedException>(async () =>
            await eventStore.GetAggregateAtAsync<User, UserStream>(headStream.Id, queryTimestamp));
    }

    [Test]
    public async Task Throws_WhenPreviousSliceIsMissing()
    {
        var t1 = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var t2 = new DateTime(2025, 1, 2, 0, 0, 0, DateTimeKind.Utc);
        var t3 = new DateTime(2025, 1, 3, 0, 0, 0, DateTimeKind.Utc);
        var queryTimestamp = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        var databaseName = await CreateDatabase();
        var eventStore = InitEventStoreBuilder(databaseName)
            .WithAggregate(typeof(User))
            .Build();

        var registeredEvent = UserRegisteredEvent.Create("event-sorcerer", "john@event-sorcerer.com", "MEMBER");
        registeredEvent.Timestamp = t1;

        var verifiedEvent = UserVerifiedEvent.Create;
        verifiedEvent.Timestamp = t2;

        var activatedEvent = UserActivatedEvent.Create;
        activatedEvent.Timestamp = t3;

        var slice1 = await eventStore.CreateStreamAndStoreAsync<UserStream>(registeredEvent, verifiedEvent);
        var headStream = await eventStore.SliceStreamAndStoreAsync<UserStream>(slice1.Id, activatedEvent);

        await Delete(databaseName, headStream.PreviousSliceId);

        Assert.ThrowsAsync<NonExistentStreamException>(async () =>
            await eventStore.GetAggregateAtAsync<User, UserStream>(headStream.Id, queryTimestamp));
    }

    [Test]
    public async Task Throws_WhenAggregateTypeIsNotRegistered()
    {
        var databaseName = await CreateDatabase();
        var eventStore = InitEventStoreBuilder(databaseName)
            .WithNoAggregateRegistered()
            .Build();

        var stream = await eventStore.CreateStreamAndStoreAsync<UserStream>(
            UserRegisteredEvent.Create("event-sorcerer", "john@event-sorcerer.com", "MEMBER"));

        Assert.ThrowsAsync<UnregisteredAggregateTypeException>(async () =>
            await eventStore.GetAggregateAtAsync<User, UserStream>(stream.Id, DateTime.UtcNow));
    }

    [Test]
    public async Task ReturnsNull_WhenTimestampIsBeforeAllEvents_AcrossAllSlices()
    {
        var queryTimestamp = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var t2 = new DateTime(2025, 1, 2, 0, 0, 0, DateTimeKind.Utc);
        var t3 = new DateTime(2025, 1, 3, 0, 0, 0, DateTimeKind.Utc);

        var databaseName = await CreateDatabase();
        var eventStore = InitEventStoreBuilder(databaseName)
            .WithAggregate(typeof(User))
            .Build();

        var registeredEvent = UserRegisteredEvent.Create("event-sorcerer", "john@event-sorcerer.com", "MEMBER");
        registeredEvent.Timestamp = t2;

        var verifiedEvent = UserVerifiedEvent.Create;
        verifiedEvent.Timestamp = t3;

        var sourceStream = await eventStore.CreateStreamAndStoreAsync<UserStream>(registeredEvent);
        var headStream = await eventStore.SliceStreamAndStoreAsync<UserStream>(sourceStream.Id, verifiedEvent);

        var aggregate = await eventStore.GetAggregateAtAsync<User, UserStream>(headStream.Id, queryTimestamp);

        Assert.That(aggregate, Is.Null);
    }
}
