using System;

namespace Raven.EventStore.Tests.Events;

public class UserVerifiedEvent : Event
{
    public override string Name => nameof(UserVerifiedEvent);
    public string Status { get; init; }

    public static UserVerifiedEvent Create()
    {
        return new UserVerifiedEvent
        {
            Status = "VERIFIED",
            EventId = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
        };
    }
}