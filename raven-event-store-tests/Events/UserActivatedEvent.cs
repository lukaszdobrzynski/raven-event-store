using System;

namespace Raven.EventStore.Tests.Events;

public class UserActivatedEvent : Event
{
    public override string Name => nameof(UserActivatedEvent);
    public string Status { get; init; }
    
    public static UserActivatedEvent Create()
    {
        return new UserActivatedEvent
        {
            Status = "ACTIVATED",
            EventId = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
        };
    }
}