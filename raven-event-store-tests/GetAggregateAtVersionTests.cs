using System;
using System.Threading.Tasks;
using Raven.EventStore.Exceptions;
using Raven.EventStore.Tests.Asserts;
using RavenEventStoreTestModels.Aggregates;
using RavenEventStoreTestModels.Events;
using RavenEventStoreTestModels.Streams;

namespace Raven.EventStore.Tests;

[Parallelizable]
public class GetAggregateAtVersionTests : TestBase
{
    [Test]
    public async Task ReturnsNull_WhenStreamDoesNotExist()
    {
        var notExistsStreamKey = Guid.NewGuid();

        var databaseName = await CreateDatabase();
        var eventStore = InitEventStoreBuilder(databaseName)
            .WithAggregate(typeof(User))
            .Build();

        var aggregate = await eventStore.GetAggregateAtVersionAsync<User, UserStream>(notExistsStreamKey, 1);

        Assert.That(aggregate, Is.Null);
    }

    [Test]
    public async Task ReturnsNull_WhenVersionIsZero_And_SingleStream()
    {
        const int aggregateAtVersion = 0;
        
        var databaseName = await CreateDatabase();
        var eventStore = InitEventStoreBuilder(databaseName)
            .WithAggregate(typeof(User))
            .Build();

        var stream = await eventStore.CreateStreamAndStoreAsync<UserStream>(
            UserRegisteredEvent.Create("event-sorcerer", "john@event-sorcerer.com", "MEMBER"));

        var aggregate = await eventStore.GetAggregateAtVersionAsync<User, UserStream>(stream.StreamKey, aggregateAtVersion);

        Assert.That(aggregate, Is.Null);
    }

    [Test]
    public async Task ReturnsNull_WhenVersionExceedsCurrentPosition()
    {
        const int aggregateAtVersion = 99;
        
        var databaseName = await CreateDatabase();
        var eventStore = InitEventStoreBuilder(databaseName)
            .WithAggregate(typeof(User))
            .Build();

        var stream = await eventStore.CreateStreamAndStoreAsync<UserStream>(
            UserRegisteredEvent.Create("event-sorcerer", "john@event-sorcerer.com", "MEMBER"),
            UserVerifiedEvent.Create,
            UserActivatedEvent.Create);
        
        StreamAssert.Position(stream, 3);

        var aggregate = await eventStore.GetAggregateAtVersionAsync<User, UserStream>(stream.StreamKey, aggregateAtVersion);

        Assert.That(aggregate, Is.Null);
    }

    [Test]
    public async Task ReturnsNull_WhenVersionDoesNotExist_AcrossAllSlices()
    {
        const int aggregateAtVersion = 0;
        
        var databaseName = await CreateDatabase();
        var eventStore = InitEventStoreBuilder(databaseName)
            .WithAggregate(typeof(User))
            .Build();

        var sourceStream = await eventStore.CreateStreamAndStoreAsync<UserStream>(UserRegisteredEvent.Create("event-sorcerer", "john@event-sorcerer.com", "MEMBER"));
        var sliceStream = await eventStore.SliceStreamAndStoreAsync<UserStream>(sourceStream.StreamKey, UserVerifiedEvent.Create);
        
        StreamAssert.Position(sourceStream, 1);
        StreamAssert.Position(sliceStream, 2);
        
        var aggregate = await eventStore.GetAggregateAtVersionAsync<User, UserStream>(sourceStream.StreamKey, aggregateAtVersion);

        Assert.That(aggregate, Is.Null);
    }

    [Test]
    public async Task GetsAggregate_VersionOne_SingleStreamWithThreeEvents()
    {
        const int aggregateAtVersion = 1;

        var databaseName = await CreateDatabase();
        var eventStore = InitEventStoreBuilder(databaseName)
            .WithAggregate(typeof(User))
            .Build();

        var stream = await eventStore.CreateStreamAndStoreAsync<UserStream>(
            UserRegisteredEvent.Create("event-sorcerer", "john@event-sorcerer.com", "MEMBER"),
            UserVerifiedEvent.Create,
            UserActivatedEvent.Create);
        
        StreamAssert.Position(stream, 3);

        var aggregate = await eventStore.GetAggregateAtVersionAsync<User, UserStream>(stream.StreamKey, aggregateAtVersion);

        Assert.That(aggregate.Username, Is.EqualTo("event-sorcerer"));
        Assert.That(aggregate.Email, Is.EqualTo("john@event-sorcerer.com"));
        Assert.That(aggregate.Status, Is.EqualTo("REGISTERED"));
        Assert.That(aggregate.Role, Is.EqualTo("MEMBER"));
    }

    [Test]
    public async Task GetsAggregate_VersionTwo_SingleStreamWithThreeEvents()
    {
        const int aggregateAtVersion = 2;

        var databaseName = await CreateDatabase();
        var eventStore = InitEventStoreBuilder(databaseName)
            .WithAggregate(typeof(User))
            .Build();

        var stream = await eventStore.CreateStreamAndStoreAsync<UserStream>(
            UserRegisteredEvent.Create("event-sorcerer", "john@event-sorcerer.com", "MEMBER"),
            UserVerifiedEvent.Create,
            UserActivatedEvent.Create);
        
        StreamAssert.Position(stream, 3);

        var aggregate = await eventStore.GetAggregateAtVersionAsync<User, UserStream>(stream.StreamKey, aggregateAtVersion);

        Assert.That(aggregate.Status, Is.EqualTo("VERIFIED"));
    }

    [Test]
    public async Task GetsAggregate_VersionThree_SingleStreamWithThreeEvents()
    {
        const int aggregateAtVersion = 3;

        var databaseName = await CreateDatabase();
        var eventStore = InitEventStoreBuilder(databaseName)
            .WithAggregate(typeof(User))
            .Build();

        var stream = await eventStore.CreateStreamAndStoreAsync<UserStream>(
            UserRegisteredEvent.Create("event-sorcerer", "john@event-sorcerer.com", "MEMBER"),
            UserVerifiedEvent.Create,
            UserActivatedEvent.Create);

        StreamAssert.Position(stream, 3);

        var aggregate = await eventStore.GetAggregateAtVersionAsync<User, UserStream>(stream.StreamKey, aggregateAtVersion);

        Assert.That(aggregate.Status, Is.EqualTo("ACTIVATED"));
    }

    [Test]
    public async Task GetsAggregate_AtVersion_InCurrentSlice()
    {
        const int aggregateAtVersion = 3;

        var databaseName = await CreateDatabase();
        var eventStore = InitEventStoreBuilder(databaseName)
            .WithAggregate(typeof(User))
            .Build();

        var sourceStream = await eventStore.CreateStreamAndStoreAsync<UserStream>(
            UserRegisteredEvent.Create("event-sorcerer", "john@event-sorcerer.com", "MEMBER"),
            UserVerifiedEvent.Create);

        var headStream = await eventStore.SliceStreamAndStoreAsync<UserStream>(sourceStream.StreamKey,
            UserActivatedEvent.Create,
            UserChangedEmailEvent.Create("john@event-sorcerer.io"));

        StreamAssert.Position(headStream, 4);

        var aggregate = await eventStore.GetAggregateAtVersionAsync<User, UserStream>(headStream.StreamKey, aggregateAtVersion);

        Assert.That(aggregate.Status, Is.EqualTo("ACTIVATED"));
        Assert.That(aggregate.Email, Is.EqualTo("john@event-sorcerer.com"));
    }

    [Test]
    public async Task GetsAggregate_AtVersion_InPreviousSlice()
    {
        const int aggregateAtVersion = 3;

        var databaseName = await CreateDatabase();
        var eventStore = InitEventStoreBuilder(databaseName)
            .WithAggregate(typeof(User))
            .Build();

        var slice1 = await eventStore.CreateStreamAndStoreAsync<UserStream>(
            UserRegisteredEvent.Create("event-sorcerer", "john@event-sorcerer.com", "MEMBER"),
            UserVerifiedEvent.Create);

        var slice2 = await eventStore.SliceStreamAndStoreAsync<UserStream>(slice1.StreamKey,
            UserActivatedEvent.Create,
            UserChangedEmailEvent.Create("john@event-sorcerer.io"));

        var headStream = await eventStore.SliceStreamAndStoreAsync<UserStream>(slice2.StreamKey,
            UserRoleChangedEvent.Create("ADMIN"));

        StreamAssert.Position(headStream, 5);

        var aggregate = await eventStore.GetAggregateAtVersionAsync<User, UserStream>(headStream.StreamKey, aggregateAtVersion);

        Assert.That(aggregate.Status, Is.EqualTo("ACTIVATED"));
        Assert.That(aggregate.Email, Is.EqualTo("john@event-sorcerer.com"));
    }

    [Test]
    public async Task GetsAggregate_AtVersion_InEarliestSlice()
    {
        const int aggregateAtVersion = 1;

        var databaseName = await CreateDatabase();
        var eventStore = InitEventStoreBuilder(databaseName)
            .WithAggregate(typeof(User))
            .Build();

        var slice1 = await eventStore.CreateStreamAndStoreAsync<UserStream>(
            UserRegisteredEvent.Create("event-sorcerer", "john@event-sorcerer.com", "MEMBER"),
            UserVerifiedEvent.Create);

        var headStream = await eventStore.SliceStreamAndStoreAsync<UserStream>(slice1.StreamKey,
            UserActivatedEvent.Create);

        StreamAssert.Position(headStream, 3);

        var aggregate = await eventStore.GetAggregateAtVersionAsync<User, UserStream>(headStream.StreamKey, aggregateAtVersion);

        Assert.That(aggregate.Username, Is.EqualTo("event-sorcerer"));
        Assert.That(aggregate.Status, Is.EqualTo("REGISTERED"));
    }

    [Test]
    public async Task Throws_WhenSeedIsMissing()
    {
        const int aggregateAtVersion = 3;

        var databaseName = await CreateDatabase();
        var eventStore = InitEventStoreBuilder(databaseName)
            .WithAggregate(typeof(User))
            .Build();

        var slice1 = await eventStore.CreateStreamAndStoreAsync<UserStream>(
            UserRegisteredEvent.Create("event-sorcerer", "john@event-sorcerer.com", "MEMBER"),
            UserVerifiedEvent.Create);

        var headStream = await eventStore.SliceStreamAndStoreAsync<UserStream>(slice1.StreamKey,
            UserActivatedEvent.Create);

        await Delete(databaseName, headStream.SeedId);

        Assert.ThrowsAsync<NonExistentSeedException>(async () =>
            await eventStore.GetAggregateAtVersionAsync<User, UserStream>(headStream.StreamKey, aggregateAtVersion));
    }

    [Test]
    public async Task Throws_WhenPreviousSlice_IsMissing()
    {
        const int aggregateAtVersion = 1;

        var databaseName = await CreateDatabase();
        var eventStore = InitEventStoreBuilder(databaseName)
            .WithAggregate(typeof(User))
            .Build();

        var slice1 = await eventStore.CreateStreamAndStoreAsync<UserStream>(
            UserRegisteredEvent.Create("event-sorcerer", "john@event-sorcerer.com", "MEMBER"),
            UserVerifiedEvent.Create);

        var headStream = await eventStore.SliceStreamAndStoreAsync<UserStream>(slice1.StreamKey,
            UserActivatedEvent.Create);

        await Delete(databaseName, headStream.PreviousSliceId);

        Assert.ThrowsAsync<NonExistentStreamException>(async () =>
            await eventStore.GetAggregateAtVersionAsync<User, UserStream>(headStream.StreamKey, aggregateAtVersion));
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
            await eventStore.GetAggregateAtVersionAsync<User, UserStream>(stream.StreamKey, 1));
    }
}
