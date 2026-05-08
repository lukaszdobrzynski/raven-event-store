using System;
using System.Collections.Generic;
using System.Linq;
using RavenEventStoreTestModels.Events;
using RavenEventStoreTestModels.Streams;

namespace Raven.EventStore.Bench;

public class StreamFactory(IRavenEventStore eventStore)
{
    private static readonly List<Func<Event>> UserEventGenerators =
    [
        () => UserRegisteredEvent.Create("event-sorcerer", "john@event-sorcerer.com", "MEMBER"),
        () => UserVerifiedEvent.Create,
        () => UserActivatedEvent.Create,
        () => UserRoleChangedEvent.Create("ADMIN"),
        () => UserChangedEmailEvent.Create("john@event-sorcerer.io"),
        () => UserDeactivatedEvent.Create,
        () => UserActivatedEvent.Create,
        () => UserRoleChangedEvent.Create("MEMBER"),
        () => UserChangedEmailEvent.Create("john@event-sorcerer.com"),
        () => UserDeactivatedEvent.Create
    ];

    public UserStream CreateUserStream(int eventsPerSlice, int sliceCount)
    {
        var stream = eventStore.CreateStreamAndStore<UserStream>(GenerateBatch());

        for (var i = 0; i < sliceCount; i++)
        {
            stream = eventStore.SliceStreamAndStore<UserStream>(stream.Id, GenerateBatch());
        }

        return stream;

        Event[] GenerateBatch() => Enumerable.Range(0, eventsPerSlice)
            .Select(i => UserEventGenerators[i % UserEventGenerators.Count]())
            .ToArray();
    }

    public UserStream CreateUserStream(int eventsPerSlice, int sliceCount, Func<int, DateTime> timestampSelector)
    {
        var eventIndex = 0;
        var stream = eventStore.CreateStreamAndStore<UserStream>(GenerateBatch());

        for (var i = 0; i < sliceCount; i++)
            stream = eventStore.SliceStreamAndStore<UserStream>(stream.Id, GenerateBatch());

        return stream;

        Event[] GenerateBatch() => Enumerable.Range(0, eventsPerSlice)
            .Select(_ =>
            {
                var e = UserEventGenerators[eventIndex % UserEventGenerators.Count]();
                e.Timestamp = timestampSelector(eventIndex++);
                return e;
            })
            .ToArray();
    }
}
