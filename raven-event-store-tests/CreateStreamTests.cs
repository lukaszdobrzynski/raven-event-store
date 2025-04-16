using System.Threading.Tasks;

namespace Raven.EventStore.Tests;

public class CreateStreamTests : TestBase
{
    [Test]
    public async Task CreatesStream_WithNoAggregate()
    {
        var database = await CreateDatabase();

        var eventStore = InitEventStoreBuilder()
            .WithDatabaseName(database)
            .Build();

        var stream = await eventStore.CreateStreamAndStoreAsync<UserStream>(database);
        var fromDb = await LoadAsync<UserStream>(database, stream.Id);
        
        Assert.That(stream, Is.Not.Null);
        Assert.That(fromDb, Is.Not.Null);
        Assert.That(fromDb.Id, Is.EqualTo(stream.Id));
        Assert.That(fromDb.StreamKey, Is.EqualTo(stream.StreamKey));
    }
}