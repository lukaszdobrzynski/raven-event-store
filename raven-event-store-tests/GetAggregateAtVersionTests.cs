using System.Threading.Tasks;
using Raven.EventStore.Exceptions;
using Raven.EventStore.Tests.Aggregates;
using Raven.EventStore.Tests.Asserts;
using Raven.EventStore.Tests.Events;
using Raven.EventStore.Tests.Streams;

namespace Raven.EventStore.Tests;

[Parallelizable]
public class GetAggregateAtVersionTests : TestBase
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

        var aggregate = await eventStore.GetAggregateAtVersionAsync<User, UserStream>(notExistsStreamId, 1);

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

        var aggregate = await eventStore.GetAggregateAtVersionAsync<User, UserStream>(stream.Id, aggregateAtVersion);

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

        var aggregate = await eventStore.GetAggregateAtVersionAsync<User, UserStream>(stream.Id, aggregateAtVersion);

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
        var sliceStream = await eventStore.SliceStreamAndStoreAsync<UserStream>(sourceStream.Id, UserVerifiedEvent.Create);
        
        StreamAssert.Position(sourceStream, 1);
        StreamAssert.Position(sliceStream, 2);
        
        var aggregate = await eventStore.GetAggregateAtVersionAsync<User, UserStream>(sourceStream.Id, aggregateAtVersion);

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

        var aggregate = await eventStore.GetAggregateAtVersionAsync<User, UserStream>(stream.Id, aggregateAtVersion);

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

        var aggregate = await eventStore.GetAggregateAtVersionAsync<User, UserStream>(stream.Id, aggregateAtVersion);

        Assert.That(aggregate.Status, Is.EqualTo("VERIFIED"));
    }

    [Test]
    public async Task GetsAggregate_CurrentVersion_SingleStreamWithThreeEvents()
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

        var aggregate = await eventStore.GetAggregateAtVersionAsync<User, UserStream>(stream.Id, aggregateAtVersion);

        Assert.That(aggregate.Status, Is.EqualTo("ACTIVATED"));
    }

    [Test]
    public async Task GetsAggregate_AtVersion_InCurrentSlice_WithSeed()
    {
        const int aggregateAtVersion = 3;

        var databaseName = await CreateDatabase();
        var eventStore = InitEventStoreBuilder(databaseName)
            .WithAggregate(typeof(User))
            .Build();

        var sourceStream = await eventStore.CreateStreamAndStoreAsync<UserStream>(
            UserRegisteredEvent.Create("event-sorcerer", "john@event-sorcerer.com", "MEMBER"),
            UserVerifiedEvent.Create);

        var headStream = await eventStore.SliceStreamAndStoreAsync<UserStream>(sourceStream.Id,
            UserActivatedEvent.Create,
            UserChangedEmailEvent.Create("john@event-sorcerer.io"));

        StreamAssert.Position(headStream, 4);

        var aggregate = await eventStore.GetAggregateAtVersionAsync<User, UserStream>(headStream.Id, aggregateAtVersion);

        Assert.That(aggregate.Status, Is.EqualTo("ACTIVATED"));
        Assert.That(aggregate.Email, Is.EqualTo("john@event-sorcerer.com"));
    }

    [Test]
    public async Task GetsAggregate_AtVersion_InPreviousSlice_WithSeed()
    {
        const int aggregateAtVersion = 3;

        var databaseName = await CreateDatabase();
        var eventStore = InitEventStoreBuilder(databaseName)
            .WithAggregate(typeof(User))
            .Build();

        var slice1 = await eventStore.CreateStreamAndStoreAsync<UserStream>(
            UserRegisteredEvent.Create("event-sorcerer", "john@event-sorcerer.com", "MEMBER"),
            UserVerifiedEvent.Create);

        var slice2 = await eventStore.SliceStreamAndStoreAsync<UserStream>(slice1.Id,
            UserActivatedEvent.Create,
            UserChangedEmailEvent.Create("john@event-sorcerer.io"));

        var headStream = await eventStore.SliceStreamAndStoreAsync<UserStream>(slice2.Id,
            UserRoleChangedEvent.Create("ADMIN"));

        StreamAssert.Position(headStream, 5);

        var aggregate = await eventStore.GetAggregateAtVersionAsync<User, UserStream>(headStream.Id, aggregateAtVersion);

        Assert.That(aggregate.Status, Is.EqualTo("ACTIVATED"));
        Assert.That(aggregate.Email, Is.EqualTo("john@event-sorcerer.com"));
    }

    [Test]
    public async Task GetsAggregate_AtVersion_InEarliestSlice_NoSeed()
    {
        const int aggregateAtVersion = 1;

        var databaseName = await CreateDatabase();
        var eventStore = InitEventStoreBuilder(databaseName)
            .WithAggregate(typeof(User))
            .Build();

        var slice1 = await eventStore.CreateStreamAndStoreAsync<UserStream>(
            UserRegisteredEvent.Create("event-sorcerer", "john@event-sorcerer.com", "MEMBER"),
            UserVerifiedEvent.Create);

        var headStream = await eventStore.SliceStreamAndStoreAsync<UserStream>(slice1.Id,
            UserActivatedEvent.Create);

        StreamAssert.Position(headStream, 3);

        var aggregate = await eventStore.GetAggregateAtVersionAsync<User, UserStream>(headStream.Id, aggregateAtVersion);

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

        var headStream = await eventStore.SliceStreamAndStoreAsync<UserStream>(slice1.Id,
            UserActivatedEvent.Create);

        await Delete(databaseName, headStream.SeedId);

        Assert.ThrowsAsync<NonExistentSeedException>(async () =>
            await eventStore.GetAggregateAtVersionAsync<User, UserStream>(headStream.Id, aggregateAtVersion));
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

        var headStream = await eventStore.SliceStreamAndStoreAsync<UserStream>(slice1.Id,
            UserActivatedEvent.Create);

        await Delete(databaseName, headStream.PreviousSliceId);

        Assert.ThrowsAsync<NonExistentStreamException>(async () =>
            await eventStore.GetAggregateAtVersionAsync<User, UserStream>(headStream.Id, aggregateAtVersion));
    }
}
