using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Raven.Client.Documents;

namespace Raven.EventStore.Perf;

public class TestRunner : IDisposable
{
    private readonly TestRunnerSettings _settings;
    private readonly IDocumentStore _store;
    
    public TestRunner(TestRunnerSettings settings)
    {
        _settings = settings;
        _store = new DocumentStoreHolder(_settings.DatabaseUrl, _settings.DatabaseName).GetStore;
    }
    
    public async Task Run(string testName, Action<RavenEventStoreConfigurationOptions> configureStoreOptions, int eventsMultiplicationFactor)
    {
        Console.WriteLine($"{testName}: Preparing test data...");
    
        _store.ConfigureEventStore(configureStoreOptions);
        var eventStore = _store.GetEventStore();
    
        var eventsToTests = CreateTestEventsSet(eventsMultiplicationFactor);
    
        Console.WriteLine($"{testName}: Running test...");
    
        await MeasureTimeAsync(testName, async () =>
        {
            await eventStore.CreateStreamAsync<UserStream>(eventsToTests);
        }, eventsToTests.Count);

        Console.WriteLine($"{testName}: Cleaning up test data...");
        await CleanupDatabase(_store);
        
        Console.WriteLine(new string('-', 50));
    }
    
    private static async Task MeasureTimeAsync(string testName, Func<Task> testMethod, int totalEvents)
    {
        var stopwatch = Stopwatch.StartNew();
        await testMethod();
        stopwatch.Stop();

        Console.WriteLine($"{testName}: Took: {stopwatch.ElapsedMilliseconds} ms with total events: {totalEvents}");
    }
    
    private static async Task CleanupDatabase(IDocumentStore store)
    {
        using (var session = store.OpenAsyncSession())
        {
            var users = await session.Query<User>().ToListAsync();
            users.ForEach(user => session.Delete(user));

            var userProjections = await session.Query<UserProjection>().ToListAsync();
            userProjections.ForEach(projection => session.Delete(projection));

            var userStreams = await session.Query<UserStream>().ToListAsync();
            userStreams.ForEach(userStream => session.Delete(userStream));

            var globalEvents = await session.Query<GlobalEventLog>().ToListAsync();
            globalEvents.ForEach(globalEvent => session.Delete(globalEvent));

            await session.SaveChangesAsync();
        }
    }
    
    private List<Event> CreateTestEventsSet(int scalingFactor) => Enumerable.Range(0, scalingFactor).SelectMany(_ => _settings.BaseEvents).ToList();

    public void Dispose()
    {
        _store?.Dispose();
    }
}

public class TestRunnerSettings
{
    public List<Event> BaseEvents { get; set; }
    public string DatabaseUrl { get; set; }
    public string DatabaseName { get; set; }
}