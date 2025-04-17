using System;

namespace Raven.EventStore.Tests.Events;

public class UserDeactivatedEvent : Event
{
    public override string Name => nameof(UserDeactivatedEvent);
    public string Status { get; init; }

    public static UserDeactivatedEvent Create()
    {
        return new UserDeactivatedEvent
        {
            Status = "DEACTIVATED",
            EventId = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
        };
    }
}