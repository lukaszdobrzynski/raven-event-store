using System;

namespace Raven.EventStore.Tests.Events;

public class UserChangedEmailEvent : Event
{
    public override string Name => nameof(UserChangedEmailEvent);
    public string Email { get; init; }

    public static UserChangedEmailEvent Create(string email)
    {
        return new UserChangedEmailEvent
        {
            Email = email,
            EventId = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow
        };
    } 
}