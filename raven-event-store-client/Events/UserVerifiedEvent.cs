using System;

namespace Raven.EventStore.Client.Events;

public class UserVerifiedEvent : Event
{
    public override string Name => nameof(UserVerifiedEvent);
    public string VerificationStatus => "VERIFIED";
    public DateTime VerifiedAt { get; private init; }

    public static UserVerifiedEvent Create => new()
    {
        EventId = Guid.NewGuid(),
        VerifiedAt = DateTime.UtcNow,
        Timestamp = DateTime.UtcNow
    };
}