using System.Threading.Tasks;
using Raven.Client.Documents;
using Raven.EventStore.Exceptions;
using Raven.EventStore.Tests.Aggregates;

namespace Raven.EventStore.Tests;

[Parallelizable]
public class ConfigurationTests : TestBase
{
    [TestCase("")]
    [TestCase(null)]
    public void Throws_WhenEventStoreDatabaseName_Is(string databaseName)
    {
        Assert.Throws<EventStoreConfigurationException>(() =>
        {
            DocumentStore.AddEventStore(options =>
            {
                options.DatabaseName = databaseName;
            });
        });
    }

    [Test]
    public void Throws_WhenMultipleEventStores_RegisteredWithSameDatabaseName()
    {
        const string dbName1 = "DB-1";
        const string dbName2 = "DB-1";
        
        DocumentStore.AddEventStore(options =>
        {
            options.DatabaseName = dbName1;
        });
        
        Assert.Throws<EventStoreConfigurationException>(() =>
        {
            DocumentStore.AddEventStore(options =>
            {
                options.DatabaseName = dbName2;
            });
        });
    }

    [Test]
    public async Task ConfiguresMultipleEventStores_WithUniqueDatabaseNames()
    {
        var dbName1 = await CreateDatabase();
        var dbName2 = await CreateDatabase();
        var dbName3 = await CreateDatabase();
        
        DocumentStore.AddEventStore(options =>
        {
            options.DatabaseName = dbName1;
        });
        
        DocumentStore.AddEventStore(options =>
        {
            options.DatabaseName = dbName2;
        });
        
        DocumentStore.AddEventStore(options =>
        {
            options.DatabaseName = dbName3;
        });
        
        var store1 = DocumentStore.GetEventStore(dbName1);
        var store2 = DocumentStore.GetEventStore(dbName2);
        var store3 = DocumentStore.GetEventStore(dbName3);
        
        Assert.That(store1, Is.Not.Null);
        Assert.That(store2, Is.Not.Null);
        Assert.That(store3, Is.Not.Null);
        
        Assert.That(store1.DatabaseName, Is.EqualTo(dbName1));
        Assert.That(store2.DatabaseName, Is.EqualTo(dbName2));
        Assert.That(store3.DatabaseName, Is.EqualTo(dbName3));
    }

    [Test]
    public async Task Throws_WhenSameAggregateType_RegisteredMoreThanOnce()
    {
        var dbName = await CreateDatabase();
        
        Assert.Throws<EventStoreConfigurationException>(() => DocumentStore.AddEventStore(options =>
        {
            options.DatabaseName = dbName;
            options.Aggregates.Register(registry =>
            {
                registry.Add<User>();
                registry.Add<User>();
            });
        }));
    }

    [Test]
    public async Task Throws_WhenAggregateType_IsNotValid()
    {
        var invalid = typeof(InvalidAggregate);
                
        var dbName = await CreateDatabase();
        
        var exception = Assert.Throws<EventStoreConfigurationException>(() => DocumentStore.AddEventStore(options =>
        {
            options.DatabaseName = dbName;
            options.Aggregates.Register(registry =>
            {
                registry.Add(invalid);
            });
        }));
        
        Assert.That(exception.Message, Does.Contain("must inherit from Aggregate<T>"));
    }

    [Test]
    public void SameDatabaseName_CanBeRegistered_OnDifferentDocumentStores()
    {
        const string dbName = "db-name";
        
        using var store1 = new DocumentStore();
        store1.Urls = ["http://localhost:8080"];
        store1.Initialize();
        
        using var store2 = new DocumentStore();
        store2.Urls = ["http://localhost:8080"];
        store2.Initialize();

        store1.AddEventStore(options => options.DatabaseName = dbName);
        store2.AddEventStore(options => options.DatabaseName = dbName);

        var eventStore1 = store1.GetEventStore(dbName);
        var eventStore2 = store2.GetEventStore(dbName);

        Assert.That(eventStore1, Is.Not.Null);
        Assert.That(eventStore2, Is.Not.Null);
        Assert.That(eventStore1, Is.Not.SameAs(eventStore2));
    }
}