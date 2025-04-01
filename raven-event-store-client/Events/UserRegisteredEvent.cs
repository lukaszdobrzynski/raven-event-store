using System;

namespace Raven.EventStore.Client;

public class UserRegisteredEvent : Event
{
    public override string Name => nameof(UserRegisteredEvent);
    public string Username { get; set; }
    public string Email { get; set; }
    public string VerificationStatus => "PENDING VERIFICATION";
    public string ActivationStatus => "INACTIVE";
    public DateTime RegisteredAt { get; private init; }

    public static UserRegisteredEvent Create(string username, string email) => new()
    {
        EventId = Guid.NewGuid(),
        Username = username,
        Email = email,
        RegisteredAt = DateTime.UtcNow,
        Timestamp = DateTime.UtcNow
    };
}