using System.Collections.Generic;
using Raven.EventStore;
using Raven.EventStore.Perf;
using Raven.EventStore.Perf.Events;

var dbUrl = args[0];
var dbName = args[1];

var baseEventsSet = new List<Event>
{
    UserRegisteredEvent.Create("event-sorcerer", "lukasz@event-driven.com"), 
    UserVerifiedEvent.Create, 
    UserActivatedEvent.Create, 
    UserChangedEmailEvent.Create("lukasz@event-driven.io"), 
    UserDeactivatedEvent.Create
};

var testRunner = new TestRunner(new TestRunnerSettings { BaseEvents = baseEventsSet, DatabaseUrl = dbUrl, DatabaseName = dbName});

await testRunner.Run("Test-1", options =>
{
    options.Projections.Add<UserProjection>();
    options.Snapshots.Add<User>();
}, 1_00);

await testRunner.Run("Test-2", options =>
{
    options.Projections.Add<UserProjection>();
    options.Snapshots.Add<User>();
}, 1_000);

await testRunner.Run("Test-3", options =>
{
    options.Projections.Add<UserProjection>();
    options.Snapshots.Add<User>();
}, 5_000);

await testRunner.Run("Test-4", options =>
{
    options.Projections.Add<UserProjection>();
    options.Snapshots.Add<User>();
}, 10_000);

await testRunner.Run("Test-5", options =>
{
    options.Projections.Add<UserProjection>();
    options.Snapshots.Add<User>();
}, 20_000);

await testRunner.Run("Test-6", options =>
{
    options.Projections.Add<UserProjection>();
    options.Snapshots.Add<User>();
}, 50_000); 

await testRunner.Run("Test-7", options =>
{
    options.Projections.Add<UserProjection>();
    options.Snapshots.Add<User>();
}, 100_000);