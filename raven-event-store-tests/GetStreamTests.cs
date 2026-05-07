using System.Threading.Tasks;
using Raven.EventStore.Tests.Asserts;
using RavenEventStoreTestModels.Events;
using RavenEventStoreTestModels.Streams;

namespace Raven.EventStore.Tests;

[Parallelizable]
public class GetStreamTests : TestBase
{
    [Test]
    public async Task GetStream_WhenStreamExists()
    {
        var databaseName = await CreateDatabase();
        var eventStore = InitEventStoreBuilder(databaseName)
            .Build();

        var stream = await eventStore.CreateStreamAndStoreAsync<UserStream>(
            UserRegisteredEvent.Create("event-sorcerer", "john@event-sorcerer.com", "MEMBER"));
        
        var loaded = await eventStore.GetStreamAsync<UserStream>(stream.Id);
        
        StreamAssert.Key(loaded, stream.StreamKey);
        StreamAssert.Position(loaded, 1);
        StreamAssert.EventsCount(loaded, 1);
    }
    
    [Test]
    public async Task ReturnsNull_WhenStreamDoesNotExist()
    {
        const string notExistsStreamId = "not-exists-stream-id";
        
        var databaseName = await CreateDatabase();
        var eventStore = InitEventStoreBuilder(databaseName)
            .Build();

        var stream = await eventStore.CreateStreamAndStoreAsync<UserStream>(
            UserRegisteredEvent.Create("event-sorcerer", "john@event-sorcerer.com", "MEMBER"));
        
        Assert.That(stream.Id, Is.Not.EqualTo(notExistsStreamId));
        
        var loaded = await eventStore.GetStreamAsync<UserStream>(notExistsStreamId);
        
        Assert.That(loaded, Is.Null);
    }
}