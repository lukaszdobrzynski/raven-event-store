using System.Threading.Tasks;
using Raven.EventStore.Exceptions;

namespace Raven.EventStore.Tests;

public class ConfigurationTests : TestBase
{
    [TestCase("")]
    [TestCase(null)]
    public async Task Throws_WhenEventStoreName_Is(string storeName)
    {
        var dbName = await CreateDatabase();

        Assert.Throws<EventStoreConfigurationException>(() =>
        {
            DocumentStore.AddEventStore(options =>
            {
                options.DatabaseName = dbName;
                options.Name = storeName;
            });
        });
    }

    [Test]
    public async Task Throws_WhenEventStoreName_IsNotUnique()
    {
        var storeName = CreateEventStoreNameUnique();
        
        var dbName1 = await CreateDatabase();
        var dbName2 = await CreateDatabase();
        var dbName3 = await CreateDatabase();
        
        DocumentStore.AddEventStore(options =>
        {
            options.DatabaseName = dbName1;
            options.Name = storeName;
        });
        
        Assert.Throws<EventStoreConfigurationException>(() =>
        {
            DocumentStore.AddEventStore(options =>
            {
                options.DatabaseName = dbName2;
                options.Name = storeName;
            });
        });
        
        Assert.Throws<EventStoreConfigurationException>(() =>
        {
            DocumentStore.AddEventStore(options =>
            {
                options.DatabaseName = dbName3;
                options.Name = storeName;
            });
        });
    }

    [Test]
    public async Task ConfiguresMultipleEventStores_WithUniqueNames_AndUniqueDatabases()
    {
        var storeName1 = CreateEventStoreNameUnique();
        var storeName2 = CreateEventStoreNameUnique();
        var storeName3 = CreateEventStoreNameUnique();
        
        var dbName1 = await CreateDatabase();
        var dbName2 = await CreateDatabase();
        var dbName3 = await CreateDatabase();
        
        DocumentStore.AddEventStore(options =>
        {
            options.DatabaseName = dbName1;
            options.Name = storeName1;
        });
        
        DocumentStore.AddEventStore(options =>
        {
            options.DatabaseName = dbName2;
            options.Name = storeName2;
        });
        
        DocumentStore.AddEventStore(options =>
        {
            options.DatabaseName = dbName3;
            options.Name = storeName3;
        });
        
        var store1 = DocumentStore.GetEventStore(storeName1);
        var store2 = DocumentStore.GetEventStore(storeName2);
        var store3 = DocumentStore.GetEventStore(storeName3);
        
        Assert.That(store1, Is.Not.Null);
        Assert.That(store2, Is.Not.Null);
        Assert.That(store3, Is.Not.Null);
        
        Assert.That(store1.Name, Is.EqualTo(storeName1));
        Assert.That(store2.Name, Is.EqualTo(storeName2));
        Assert.That(store3.Name, Is.EqualTo(storeName3));
    }

    [Test]
    public async Task Throws_WhenSameAggregateType_RegisteredMoreThanOnce()
    {
        var storeName = CreateEventStoreNameUnique();
        var dbName = await CreateDatabase();
        
        Assert.Throws<EventStoreConfigurationException>(() => DocumentStore.AddEventStore(options =>
        {
            options.DatabaseName = dbName;
            options.Name = storeName;
            options.Aggregates.Register(registry =>
            {
                registry.Add<User>();
                registry.Add<User>();
            });
        }));
    }

    [Test]
    public async Task Throws_WhenMultipleEventStores_RegisteredWithSameDatabase()
    {
        var dbName = await CreateDatabase();
        
        var storeName1 = CreateEventStoreNameUnique();
        var storeName2 = CreateEventStoreNameUnique();
        var storeName3 = CreateEventStoreNameUnique();
        
        DocumentStore.AddEventStore(options =>
        {
            options.DatabaseName = dbName;
            options.Name = storeName1;
        });
        
        Assert.Throws<EventStoreConfigurationException>(() =>
        {
            DocumentStore.AddEventStore(options =>
            {
                options.DatabaseName = dbName;
                options.Name = storeName2;
            });
        });
        
        Assert.Throws<EventStoreConfigurationException>(() =>
        {
            DocumentStore.AddEventStore(options =>
            {
                options.DatabaseName = dbName;
                options.Name = storeName3;
            });
        });
    }
}