using System.Threading.Tasks;
using Raven.EventStore.Tests.Asserts;
using RavenEventStoreTestModels.Aggregates;
using RavenEventStoreTestModels.Events;
using RavenEventStoreTestModels.Streams;

namespace Raven.EventStore.Tests;

[Parallelizable]
public class GetAggregateTests : TestBase
{
    [Test]
    public async Task GetsAggregate_WhenAggregateRegistered_And_StreamExists()
    {
        var databaseName = await CreateDatabase();
        var eventStore = InitEventStoreBuilder(databaseName)
            .WithAggregate(typeof(User))
            .Build();

        var stream = await eventStore.CreateStreamAndStoreAsync<UserStream>(
            UserRegisteredEvent.Create("event-sorcerer", "john@event-sorcerer.com", "MEMBER"));
        
        var loaded = await eventStore.GetAggregateAsync<User>(stream.Id);
        
        AggregateAssert.AggregateId(loaded, stream.AggregateId);
        AggregateAssert.StreamKey(loaded, stream.StreamKey);
    }
    
    [Test]
    public async Task ReturnsNull_WhenAggregateRegistered_And_StreamDoesNotExist()
    {
        const string notExistsStreamId = "not-exists-stream-id";
        
        var databaseName = await CreateDatabase();
        var eventStore = InitEventStoreBuilder(databaseName)
            .WithAggregate(typeof(User))
            .Build();

        var aggregate = await eventStore.GetAggregateAsync<User>(notExistsStreamId);
        
        Assert.That(aggregate, Is.Null);
    }
    
    [Test]
    public async Task ReturnsNull_WhenAggregateNotRegistered_And_StreamExists()
    {
        var databaseName = await CreateDatabase();
        var eventStore = InitEventStoreBuilder(databaseName)
            .Build();
        
        var stream = await eventStore.CreateStreamAndStoreAsync<UserStream>(
            UserRegisteredEvent.Create("event-sorcerer", "john@event-sorcerer.com", "MEMBER"));

        var aggregate = await eventStore.GetAggregateAsync<User>(stream.AggregateId);
        
        Assert.That(aggregate, Is.Null);
    }
}