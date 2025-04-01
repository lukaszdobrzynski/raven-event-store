using System;

namespace Raven.EventStore.Client.Events;

public class UserChangedEmailEvent : Event
{
    public override string Name => nameof(UserChangedEmailEvent);
    
    public string Email { get; set; }
    
    public static UserChangedEmailEvent Create(string email) => new()
    {
        EventId = Guid.NewGuid(),
        Email = email,
        Timestamp = DateTime.UtcNow
    };
}