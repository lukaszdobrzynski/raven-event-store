using System;

namespace Raven.EventStore.Perf.Events;

public class UserDeactivatedEvent : Event
{
    public override string Name => nameof(UserDeactivatedEvent);
    public string ActivationStatus => "DEACTIVATED";
    public DateTime DeactivatedAt { get; private init; }

    public static UserDeactivatedEvent Create => new()
    {
        EventId = Guid.NewGuid(),
        DeactivatedAt = DateTime.UtcNow,
        Timestamp = DateTime.UtcNow,
    };
}