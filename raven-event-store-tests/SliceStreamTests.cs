using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Raven.EventStore.Exceptions;
using Raven.EventStore.Tests.Aggregates;
using Raven.EventStore.Tests.Asserts;
using Raven.EventStore.Tests.Events;
using Raven.EventStore.Tests.Streams;

namespace Raven.EventStore.Tests;

[Parallelizable]
public class SliceStreamTests : TestBase
{
    [Test]
    public async Task Throws_WhenSourceStream_DoesNotExist()
    {
        const string sourceStreamId = "DOES-NOT-EXIST";
        
        var databaseName = await CreateDatabase();

        await AssertNoDocumentInDb<UserStream>(databaseName, sourceStreamId);
        
        var eventStore = InitEventStoreBuilder(databaseName)
            .Build();
        
        Assert.ThrowsAsync<NonExistentStreamException>(async () => 
            await eventStore.SliceStreamAndStoreAsync<UserStream>(sourceStreamId, "NEW-STREAM-ID"));
    }

    [Test]
    public async Task Throws_WhenEventsAreNull()
    {
        List<Event> newStreamEvents = null;
        
        var databaseName = await CreateDatabase();
        
        var eventStore = InitEventStoreBuilder(databaseName)
            .Build();

        var sourceStreamId = await CreateSemanticId<UserStream>(databaseName, "2025-04");
        var nextStreamId = CreateSliceStreamNextId(sourceStreamId, "2025-05");
        
        await eventStore.CreateStreamAndStoreAsync<UserStream>(sourceStreamId);
        
        Assert.ThrowsAsync<ArgumentException>(async () => 
            await eventStore.SliceStreamAndStoreAsync<UserStream>(sourceStreamId, nextStreamId, newStreamEvents));
        
        await AssertNoDocumentInDb<UserStream>(databaseName, nextStreamId);
        
        var sourceStream = await LoadAsync<UserStream>(databaseName, sourceStreamId);
        
        StreamAssert.EventsCount(sourceStream, 0);
        StreamAssert.Position(sourceStream, 0);
        StreamAssert.ArchiveNull(sourceStream);
        StreamAssert.SeedNull(sourceStream);
        StreamAssert.AggregateIdNull(sourceStream);
        StreamAssert.IsHead(sourceStream);
    }

    [Test]
    public async Task Throws_WhenEvents_ContainNull()
    {
        var event1 = UserRoleChangedEvent.Create("ADMIN");
        Event event2 = null;
        
        var databaseName = await CreateDatabase();
        var eventStore = InitEventStoreBuilder(databaseName)
            .Build();
        
        var sourceStreamId = await CreateSemanticId<UserStream>(databaseName, "2025-04");
        var nextStreamId = CreateSliceStreamNextId(sourceStreamId, "2025-05");
        
        await eventStore.CreateStreamAndStoreAsync<UserStream>(sourceStreamId);
        
        var exception = Assert.ThrowsAsync<ArgumentException>(async () => 
            await eventStore.SliceStreamAndStoreAsync<UserStream>(sourceStreamId, nextStreamId, [event1, event2]));
        
        Assert.That(exception.Message, Does.Contain("Null event found at index 1"));
        
        await AssertNoDocumentInDb<UserStream>(databaseName, nextStreamId);
        
        var sourceStream = await LoadAsync<UserStream>(databaseName, sourceStreamId);
        
        StreamAssert.EventsCount(sourceStream, 0);
        StreamAssert.Position(sourceStream, 0);
        StreamAssert.ArchiveNull(sourceStream);
        StreamAssert.SeedNull(sourceStream);
        StreamAssert.AggregateIdNull(sourceStream);
        StreamAssert.IsHead(sourceStream);
    }

    [Test]
    public async Task CreatesSliceStream_WithNoEvents_InSourceStream_AndNextStream()
    {
        var databaseName = await CreateDatabase();
        var eventStore = InitEventStoreBuilder(databaseName)
            .Build();
        
        var sourceStreamId = await CreateSemanticId<UserStream>(databaseName, "2025-04");
        var nextStreamId = CreateSliceStreamNextId(sourceStreamId, "2025-05");
        
        await eventStore.CreateStreamAndStoreAsync<UserStream>(sourceStreamId);
        
        await eventStore.SliceStreamAndStoreAsync<UserStream>(sourceStreamId, nextStreamId);
        
        var sourceStream = await LoadAsync<UserStream>(databaseName, sourceStreamId);
        var nextStream = await LoadAsync<UserStream>(databaseName, nextStreamId);
        
        StreamAssert.Key(nextStream, sourceStream.StreamKey);
        StreamAssert.Position(sourceStream, 0);
        StreamAssert.Position(nextStream, 0);
        StreamAssert.ArchiveNull(sourceStream);
        StreamAssert.SeedNull(nextStream);
        StreamAssert.AggregateIdNull(sourceStream);
        StreamAssert.AggregateIdNull(nextStream);
        
        StreamAssert.IsNotHead(sourceStream);
        StreamAssert.IsHead(nextStream);
        StreamAssert.PreviousSliceId(sourceStream, null);
        StreamAssert.NextSliceId(sourceStream, nextStreamId);
        StreamAssert.PreviousSliceId(nextStream, sourceStreamId);
        StreamAssert.NextSliceId(nextStream, null);
    }

    [Test]
    public async Task CreatesSliceStream_WithEvents_InSourceStream_AndNextStream_ButNoAggregate()
    {
        var databaseName = await CreateDatabase();
        var eventStore = InitEventStoreBuilder(databaseName)
            .Build();
        
        var sourceStreamId = await CreateSemanticId<UserStream>(databaseName, "2025-04");
        var nextStreamId = CreateSliceStreamNextId(sourceStreamId, "2025-05");
        
        var registered = UserRegisteredEvent.Create("event-sorcerer", "john@event-sorcerer.com", "MEMBER");
        var sourceStream = await eventStore.CreateStreamAndStoreAsync<UserStream>(sourceStreamId, registered);
        
        var verified = UserVerifiedEvent.Create;
        var nextStream = await eventStore.SliceStreamAndStoreAsync<UserStream>(sourceStreamId, nextStreamId, verified);
        
        StreamAssert.Key(nextStream, sourceStream.StreamKey);
        StreamAssert.Position(sourceStream, 1);
        StreamAssert.Position(nextStream, 2);
        StreamAssert.ArchiveNull(sourceStream);
        StreamAssert.SeedNull(nextStream);
        StreamAssert.EventsCount(sourceStream, 1);
        StreamAssert.EventsCount(nextStream, 1);
        StreamAssert.AggregateIdNull(sourceStream);
        StreamAssert.AggregateIdNull(nextStream);
        
        var sourceStreamFromDb = await LoadAsync<UserStream>(databaseName, sourceStreamId);
        
        StreamAssert.IsNotHead(sourceStreamFromDb);
        StreamAssert.IsHead(nextStream);
        StreamAssert.PreviousSliceId(sourceStreamFromDb, null);
        StreamAssert.NextSliceId(sourceStreamFromDb, nextStreamId);
        StreamAssert.PreviousSliceId(nextStream, sourceStreamId);
        StreamAssert.NextSliceId(nextStream, null);
        
        EventAssert.Version(sourceStream.Events[0], 1);
        EventAssert.Version(nextStream.Events[0], 2);
        EventAssert.Type<UserRegisteredEvent>(sourceStream.Events[0]);
        EventAssert.Type<UserVerifiedEvent>(nextStream.Events[0]);
    }

    [Test]
    public async Task CreatesSliceStream_WithAggregate()
    {
        var databaseName = await CreateDatabase();
        var eventStore = InitEventStoreBuilder(databaseName)
            .WithAggregate(typeof(User))
            .Build();
        
        var sourceStreamId = await CreateSemanticId<UserStream>(databaseName, "2025-04");
        var sourceStream = await eventStore.CreateStreamAndStoreAsync<UserStream>(sourceStreamId, 
            UserRegisteredEvent.Create("event-sorcerer", "john@event-sorcerer.com", "MEMBER"),
            UserVerifiedEvent.Create,
            UserActivatedEvent.Create);
        
        var sliceStream1Id = CreateSliceStreamNextId(sourceStreamId, "2025-05");
        var sliceStream1 = await eventStore.SliceStreamAndStoreAsync<UserStream>(sourceStreamId, sliceStream1Id, 
            UserChangedEmailEvent.Create("alice@event-sorcerer.io"),
                UserRoleChangedEvent.Create("ADMIN"));
        
        var sliceStream2Id = CreateSliceStreamNextId(sliceStream1Id, "2025-06");
        var sliceStream2 = await eventStore.SliceStreamAndStoreAsync<UserStream>(sliceStream1Id, sliceStream2Id,
                UserDeactivatedEvent.Create);
        
        StreamAssert.Key(sliceStream1, sourceStream.StreamKey);
        StreamAssert.Key(sliceStream2, sourceStream.StreamKey);
        
        StreamAssert.Position(sourceStream, 3);
        StreamAssert.Position(sliceStream1, 5);
        StreamAssert.Position(sliceStream2, 6);
        
        var sourceStreamFromDb = await LoadAsync<UserStream>(databaseName, sourceStreamId);
        var sliceStream1FromDb = await LoadAsync<UserStream>(databaseName, sliceStream1Id);
        var sliceStream2FromDb = await LoadAsync<UserStream>(databaseName, sliceStream2Id);
        
        StreamAssert.ArchiveNotNull(sourceStreamFromDb);
        StreamAssert.ArchiveNotNull(sliceStream1FromDb);
        StreamAssert.ArchiveNull(sliceStream2FromDb);
        StreamAssert.SeedNull(sourceStreamFromDb);
        StreamAssert.SeedNotNull(sliceStream1FromDb);
        StreamAssert.SeedNotNull(sliceStream2FromDb);
        
        StreamAssert.IsNotHead(sourceStreamFromDb);
        StreamAssert.IsNotHead(sliceStream1FromDb);
        StreamAssert.IsHead(sliceStream2FromDb);
        StreamAssert.PreviousSliceId(sourceStreamFromDb, null);
        StreamAssert.PreviousSliceId(sliceStream1FromDb, sourceStreamId);
        StreamAssert.PreviousSliceId(sliceStream2FromDb, sliceStream1Id);
        StreamAssert.NextSliceId(sourceStreamFromDb, sliceStream1FromDb.Id);
        StreamAssert.NextSliceId(sliceStream1FromDb, sliceStream2FromDb.Id);
        StreamAssert.NextSliceId(sliceStream2FromDb, null);
        
        var sourceStreamArchive = (User)sourceStreamFromDb.Archive;
        var sliceStream1Archive = (User)sliceStream1FromDb.Archive;
        
        Assert.That(sourceStreamArchive.Username, Is.EqualTo("event-sorcerer"));
        Assert.That(sourceStreamArchive.Email, Is.EqualTo("john@event-sorcerer.com"));
        Assert.That(sourceStreamArchive.Role, Is.EqualTo("MEMBER"));
        Assert.That(sourceStreamArchive.Status, Is.EqualTo("ACTIVATED"));
        
        Assert.That(sliceStream1Archive.Username, Is.EqualTo("event-sorcerer"));
        Assert.That(sliceStream1Archive.Email, Is.EqualTo("alice@event-sorcerer.io"));
        Assert.That(sliceStream1Archive.Role, Is.EqualTo("ADMIN"));
        Assert.That(sliceStream1Archive.Status, Is.EqualTo("ACTIVATED"));
        
        var sliceStream1Seed = (User)sliceStream1FromDb.Seed;
        var sliceStream2Seed = (User)sliceStream2FromDb.Seed;
        
        Assert.That(sliceStream1Seed.Username, Is.EqualTo("event-sorcerer"));
        Assert.That(sliceStream1Seed.Email, Is.EqualTo("john@event-sorcerer.com"));
        Assert.That(sliceStream1Seed.Role, Is.EqualTo("MEMBER"));
        Assert.That(sliceStream1Seed.Status, Is.EqualTo("ACTIVATED"));
        
        Assert.That(sliceStream2Seed.Username, Is.EqualTo("event-sorcerer"));
        Assert.That(sliceStream2Seed.Email, Is.EqualTo("alice@event-sorcerer.io"));
        Assert.That(sliceStream2Seed.Role, Is.EqualTo("ADMIN"));
        Assert.That(sliceStream2Seed.Status, Is.EqualTo("ACTIVATED"));
        
        var aggregate = await LoadSingleAsync<User>(databaseName);
        
        AggregateAssert.AggregateId(aggregate, sourceStream.AggregateId);
        AggregateAssert.AggregateId(aggregate, sliceStream1.AggregateId);
        AggregateAssert.AggregateId(aggregate, sliceStream2.AggregateId);
        AggregateAssert.StreamKey(aggregate, sourceStream.StreamKey);
        
        Assert.That(aggregate.Username, Is.EqualTo("event-sorcerer"));
        Assert.That(aggregate.Email, Is.EqualTo("alice@event-sorcerer.io"));
        Assert.That(aggregate.Role, Is.EqualTo("ADMIN"));
        Assert.That(aggregate.Status, Is.EqualTo("DEACTIVATED"));
    }

    [Test]
    public async Task Throws_WhenSourceStream_IsNotHead()
    {
        var databaseName = await CreateDatabase();
        var eventStore = InitEventStoreBuilder(databaseName)
            .Build();
        
        var sourceStreamId = await CreateSemanticId<UserStream>(databaseName, "2025-04");
        await eventStore.CreateStreamAndStoreAsync<UserStream>(sourceStreamId, 
            UserRegisteredEvent.Create("event-sorcerer", "john@event-sorcerer.com", "MEMBER"),
            UserVerifiedEvent.Create,
            UserActivatedEvent.Create);
        
        var sliceStream1Id = CreateSliceStreamNextId(sourceStreamId, "2025-05");
        await eventStore.SliceStreamAndStoreAsync<UserStream>(sourceStreamId, sliceStream1Id, 
            UserChangedEmailEvent.Create("alice@event-sorcerer.io"),
            UserRoleChangedEvent.Create("ADMIN"));
        
        var sourceStreamFromDbBefore = await LoadAsync<UserStream>(databaseName, sourceStreamId);
        var sliceStream1FromDbBefore = await LoadAsync<UserStream>(databaseName, sliceStream1Id);
        
        StreamAssert.IsNotHead(sourceStreamFromDbBefore);
        StreamAssert.IsHead(sliceStream1FromDbBefore);
        
        StreamAssert.PreviousSliceId(sourceStreamFromDbBefore, null);
        StreamAssert.PreviousSliceId(sliceStream1FromDbBefore, sourceStreamId);
        StreamAssert.NextSliceId(sourceStreamFromDbBefore, sliceStream1FromDbBefore.Id);
        StreamAssert.NextSliceId(sliceStream1FromDbBefore, null);
        
        StreamAssert.Position(sourceStreamFromDbBefore, 3);
        StreamAssert.Position(sliceStream1FromDbBefore, 5);
        
        var sliceStream2Id = CreateSliceStreamNextId(sliceStream1Id, "2025-06");

        Assert.ThrowsAsync<CreateSliceStreamFromNotHeadException>(async () =>
            await eventStore.SliceStreamAndStoreAsync<UserStream>(sourceStreamId, sliceStream2Id,
                UserDeactivatedEvent.Create));
        
        await AssertNoDocumentInDb<UserStream>(databaseName, sliceStream2Id);

        var sourceStreamFromDbAfter = await LoadAsync<UserStream>(databaseName, sourceStreamId);
        var sliceStream1FromDbAfter = await LoadAsync<UserStream>(databaseName, sliceStream1Id);
        
        StreamAssert.IsNotHead(sourceStreamFromDbAfter);
        StreamAssert.IsHead(sliceStream1FromDbAfter);
        
        StreamAssert.PreviousSliceId(sourceStreamFromDbAfter, sourceStreamFromDbBefore.PreviousSliceId);
        StreamAssert.PreviousSliceId(sliceStream1FromDbAfter, sliceStream1FromDbBefore.PreviousSliceId);
        StreamAssert.NextSliceId(sourceStreamFromDbAfter, sourceStreamFromDbBefore.NextSliceId);
        StreamAssert.NextSliceId(sliceStream1FromDbAfter, sliceStream1FromDbBefore.NextSliceId);
        
        StreamAssert.Position(sourceStreamFromDbAfter, sourceStreamFromDbAfter.Position);
        StreamAssert.Position(sliceStream1FromDbAfter, sliceStream1FromDbAfter.Position);
    }
}