using System;

namespace Raven.EventStore.Tests.Events;

public class UserRegisteredEvent : Event
{
    public override string Name => nameof(UserRegisteredEvent);
    public string Username { get; init; }
    public string Email { get; init; }
    public string Status { get; init; }
    public string Role { get; init; }

    public static UserRegisteredEvent Create(string username, string email, string role)
    {
        return new UserRegisteredEvent
        {
            Username = username,
            Email = email,
            Status = "REGISTERED",
            Role = role,
            EventId = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
        };
    }
}