using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Raven.EventStore.Exceptions;
using Raven.EventStore.Tests.Asserts;
using RavenEventStoreTestModels.Aggregates;
using RavenEventStoreTestModels.Events;
using RavenEventStoreTestModels.Streams;

namespace Raven.EventStore.Tests;

[Parallelizable]
public class SliceStreamTests : TestBase
{
    [Test]
    public async Task Throws_WhenStreamChain_DoesNotExist()
    {
        var databaseName = await CreateDatabase();

        var eventStore = InitEventStoreBuilder(databaseName)
            .Build();

        var nonExistentStreamKey = Guid.NewGuid();

        Assert.ThrowsAsync<NonExistentStreamException>(async () =>
            await eventStore.SliceStreamAndStoreAsync<UserStream>(nonExistentStreamKey, "NEW-STREAM-ID", UserActivatedEvent.Create));
    }

    [Test]
    public async Task Throws_WhenEventsAreNull_InNextStream()
    {
        List<Event> newStreamEvents = null;
        
        var databaseName = await CreateDatabase();
        
        var eventStore = InitEventStoreBuilder(databaseName)
            .Build();

        var sourceStreamId = await CreateSemanticId<UserStream>(databaseName, "2025-04");
        var nextStreamId = CreateSliceStreamNextId(sourceStreamId, "2025-05");
        
        var sourceStream = await eventStore.CreateStreamAndStoreAsync<UserStream>(sourceStreamId, UserActivatedEvent.Create);

        Assert.ThrowsAsync<ArgumentException>(async () =>
            await eventStore.SliceStreamAndStoreAsync<UserStream>(sourceStream.StreamKey, nextStreamId, newStreamEvents));

        await AssertNoDocumentInDb<UserStream>(databaseName, nextStreamId);

        var sourceStreamFromDb = await LoadAsync<UserStream>(databaseName, sourceStreamId);

        StreamAssert.EventsCount(sourceStreamFromDb, 1);
        StreamAssert.Position(sourceStreamFromDb, 1);
        StreamAssert.ArchiveNull(sourceStreamFromDb);
        StreamAssert.SeedNull(sourceStreamFromDb);
        StreamAssert.AggregateIdNull(sourceStreamFromDb);
        StreamAssert.IsHead(sourceStreamFromDb);
    }

    [Test]
    public async Task Throws_WhenEventsAreEmpty_InNextStream()
    {
        List<Event> newStreamEvents = [];

        var databaseName = await CreateDatabase();

        var eventStore = InitEventStoreBuilder(databaseName)
            .Build();

        var sourceStreamId = await CreateSemanticId<UserStream>(databaseName, "2025-04");
        var nextStreamId = CreateSliceStreamNextId(sourceStreamId, "2025-05");

        var sourceStream = await eventStore.CreateStreamAndStoreAsync<UserStream>(sourceStreamId, UserActivatedEvent.Create);

        Assert.ThrowsAsync<ArgumentException>(async () =>
            await eventStore.SliceStreamAndStoreAsync<UserStream>(sourceStream.StreamKey, nextStreamId, newStreamEvents));

        await AssertNoDocumentInDb<UserStream>(databaseName, nextStreamId);

        var sourceStreamFromDb = await LoadAsync<UserStream>(databaseName, sourceStreamId);

        StreamAssert.EventsCount(sourceStreamFromDb, 1);
        StreamAssert.Position(sourceStreamFromDb, 1);
        StreamAssert.ArchiveNull(sourceStreamFromDb);
        StreamAssert.SeedNull(sourceStreamFromDb);
        StreamAssert.AggregateIdNull(sourceStreamFromDb);
        StreamAssert.IsHead(sourceStreamFromDb);
    }

    [Test]
    public async Task Throws_WhenEventsContainNull_InNextStream()
    {
        List<Event> newStreamEvents = [UserVerifiedEvent.Create, null];

        var databaseName = await CreateDatabase();

        var eventStore = InitEventStoreBuilder(databaseName)
            .Build();

        var sourceStreamId = await CreateSemanticId<UserStream>(databaseName, "2025-04");
        var nextStreamId = CreateSliceStreamNextId(sourceStreamId, "2025-05");

        var sourceStream = await eventStore.CreateStreamAndStoreAsync<UserStream>(sourceStreamId, UserActivatedEvent.Create);

        Assert.ThrowsAsync<ArgumentException>(async () =>
            await eventStore.SliceStreamAndStoreAsync<UserStream>(sourceStream.StreamKey, nextStreamId, newStreamEvents));

        await AssertNoDocumentInDb<UserStream>(databaseName, nextStreamId);

        var sourceStreamFromDb = await LoadAsync<UserStream>(databaseName, sourceStreamId);

        StreamAssert.EventsCount(sourceStreamFromDb, 1);
        StreamAssert.Position(sourceStreamFromDb, 1);
        StreamAssert.ArchiveNull(sourceStreamFromDb);
        StreamAssert.SeedNull(sourceStreamFromDb);
        StreamAssert.AggregateIdNull(sourceStreamFromDb);
        StreamAssert.IsHead(sourceStreamFromDb);
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
        var nextStream = await eventStore.SliceStreamAndStoreAsync<UserStream>(sourceStream.StreamKey, nextStreamId, verified);
        
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
    public async Task CreatesSliceStream_WithAutoAssignedId()
    {
        var databaseName = await CreateDatabase();
        var eventStore = InitEventStoreBuilder(databaseName)
            .Build();

        var registered = UserRegisteredEvent.Create("event-sorcerer", "john@event-sorcerer.com", "MEMBER");
        var sourceStream = await eventStore.CreateStreamAndStoreAsync<UserStream>(registered);

        var verified = UserVerifiedEvent.Create;
        var nextStream = await eventStore.SliceStreamAndStoreAsync<UserStream>(sourceStream.StreamKey, verified);

        StreamAssert.IdNotNull(nextStream);
        Assert.That(nextStream.Id, Is.Not.EqualTo(sourceStream.Id));

        StreamAssert.Key(nextStream, sourceStream.StreamKey);
        StreamAssert.Position(sourceStream, 1);
        StreamAssert.Position(nextStream, 2);
        StreamAssert.EventsCount(sourceStream, 1);
        StreamAssert.EventsCount(nextStream, 1);

        var sourceStreamFromDb = await LoadAsync<UserStream>(databaseName, sourceStream.Id);
        var nextStreamFromDb = await LoadAsync<UserStream>(databaseName, nextStream.Id);

        StreamAssert.IsNotHead(sourceStreamFromDb);
        StreamAssert.IsHead(nextStreamFromDb);
        StreamAssert.PreviousSliceId(sourceStreamFromDb, null);
        StreamAssert.NextSliceId(sourceStreamFromDb, nextStream.Id);
        StreamAssert.PreviousSliceId(nextStreamFromDb, sourceStream.Id);
        StreamAssert.NextSliceId(nextStreamFromDb, null);

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
        var sliceStream1 = await eventStore.SliceStreamAndStoreAsync<UserStream>(sourceStream.StreamKey, sliceStream1Id,
            UserChangedEmailEvent.Create("alice@event-sorcerer.io"),
            UserRoleChangedEvent.Create("ADMIN"));

        var sliceStream2Id = CreateSliceStreamNextId(sliceStream1Id, "2025-06");
        var sliceStream2 = await eventStore.SliceStreamAndStoreAsync<UserStream>(sliceStream1.StreamKey, sliceStream2Id,
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
        
        var sourceStreamArchiveDoc = await LoadAsync<SliceStreamArchive>(databaseName, sourceStreamFromDb.ArchiveId);
        var sliceStream1ArchiveDoc = await LoadAsync<SliceStreamArchive>(databaseName, sliceStream1FromDb.ArchiveId);

        var sourceStreamArchive = (User)sourceStreamArchiveDoc.State;
        var sliceStream1Archive = (User)sliceStream1ArchiveDoc.State;
        
        Assert.That(sourceStreamArchive.Username, Is.EqualTo("event-sorcerer"));
        Assert.That(sourceStreamArchive.Email, Is.EqualTo("john@event-sorcerer.com"));
        Assert.That(sourceStreamArchive.Role, Is.EqualTo("MEMBER"));
        Assert.That(sourceStreamArchive.Status, Is.EqualTo("ACTIVATED"));
        
        Assert.That(sliceStream1Archive.Username, Is.EqualTo("event-sorcerer"));
        Assert.That(sliceStream1Archive.Email, Is.EqualTo("alice@event-sorcerer.io"));
        Assert.That(sliceStream1Archive.Role, Is.EqualTo("ADMIN"));
        Assert.That(sliceStream1Archive.Status, Is.EqualTo("ACTIVATED"));
        
        var sliceStream1SeedDoc = await LoadAsync<SliceStreamSeed>(databaseName, sliceStream1FromDb.SeedId);
        var sliceStream2SeedDoc = await LoadAsync<SliceStreamSeed>(databaseName, sliceStream2FromDb.SeedId);

        var sliceStream1Seed = (User)sliceStream1SeedDoc.State;
        var sliceStream2Seed = (User)sliceStream2SeedDoc.State;
        
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
    public async Task Throws_WhenSourceStreamReferencesAggregate_But_AggregateDoesNotExist()
    {
        var databaseName = await CreateDatabase();
        var eventStore = InitEventStoreBuilder(databaseName)
            .WithAggregate(typeof(User))
            .Build();

        var sourceStream = eventStore.CreateStreamAndStore<UserStream>(
            UserRegisteredEvent.Create("event-sorcerer", "john@event-sorcerer.com", "MEMBER"));

        await AssertDocumentExistsInDb<User>(databaseName, sourceStream.AggregateId);
        await Delete(databaseName, sourceStream.AggregateId);

        Assert.ThrowsAsync<NonExistentAggregateException>(async () =>
            await eventStore.SliceStreamAndStoreAsync<UserStream>(sourceStream.StreamKey, UserVerifiedEvent.Create));
    }

    [Test]
    public async Task StreamHeader_AddsSliceDescriptor_AfterFirstSlice()
    {
        var databaseName = await CreateDatabase();
        var eventStore = InitEventStoreBuilder(databaseName)
            .Build();

        var sourceStream = await eventStore.CreateStreamAndStoreAsync<UserStream>(
            UserRegisteredEvent.Create("event-sorcerer", "john@event-sorcerer.com", "MEMBER"),
            UserVerifiedEvent.Create);

        await eventStore.SliceStreamAndStoreAsync<UserStream>(sourceStream.StreamKey,
            UserActivatedEvent.Create);
        
        var sliceStreamHeader = await LoadSingleAsync<StreamHeader>(databaseName);
        
        StreamHeaderAssert.SliceDescriptorsCount(sliceStreamHeader, 1);
        StreamHeaderAssert.SliceDescriptor(sliceStreamHeader, 0, sourceStream.Id, 1, sourceStream.Events[0].Timestamp);
    }

    [Test]
    public async Task StreamHeader_AccumulatesSliceDescriptors_AfterMultipleSlices()
    {
        var databaseName = await CreateDatabase();
        var eventStore = InitEventStoreBuilder(databaseName)
            .Build();

        var slice1 = await eventStore.CreateStreamAndStoreAsync<UserStream>(
            UserRegisteredEvent.Create("event-sorcerer", "john@event-sorcerer.com", "MEMBER"),
            UserVerifiedEvent.Create);

        var slice2 = await eventStore.SliceStreamAndStoreAsync<UserStream>(slice1.StreamKey,
            UserActivatedEvent.Create,
            UserChangedEmailEvent.Create("john@event-sorcerer.io"));

        await eventStore.SliceStreamAndStoreAsync<UserStream>(slice2.StreamKey,
            UserRoleChangedEvent.Create("ADMIN"));
        
        var sliceStreamHeader = await LoadSingleAsync<StreamHeader>(databaseName);

        StreamHeaderAssert.SliceDescriptorsCount(sliceStreamHeader, 2);
        StreamHeaderAssert.SliceDescriptor(sliceStreamHeader, 0, slice1.Id, 1, slice1.Events[0].Timestamp);
        StreamHeaderAssert.SliceDescriptor(sliceStreamHeader, 1, slice2.Id, 3, slice2.Events[0].Timestamp);
    }

}