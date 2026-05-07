using System;
using Raven.EventStore;

namespace RavenEventStoreTestModels.Events;

public class UserVerifiedEvent : Event
{
    public override string Name => nameof(UserVerifiedEvent);
    public string Status { get; init; }

    public static UserVerifiedEvent Create => new ()
    {
        Status = "VERIFIED", 
        EventId = Guid.NewGuid(), 
        Timestamp = DateTime.UtcNow
    };
}