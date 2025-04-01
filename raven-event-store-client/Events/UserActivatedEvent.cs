using System;

namespace Raven.EventStore.Client.Events;

public class UserActivatedEvent : Event
{
    public override string Name => nameof(UserActivatedEvent);
    public string ActivationStatus => "ACTIVATED";
    public DateTime ActivatedAt { get; private init; }

    public static UserActivatedEvent Create => new()
    {
        EventId = Guid.NewGuid(),
        ActivatedAt = DateTime.UtcNow,
        Timestamp = DateTime.UtcNow
    };
}