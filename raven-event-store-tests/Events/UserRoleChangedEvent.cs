using System;

namespace Raven.EventStore.Tests.Events;

public class UserRoleChangedEvent : Event
{
    public override string Name => nameof(UserRoleChangedEvent);
    public string Role { get; init; }

    public static UserRoleChangedEvent Create(string role)
    {
        return new UserRoleChangedEvent()
        {
            Role = role,
            EventId = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
        };
    }
}