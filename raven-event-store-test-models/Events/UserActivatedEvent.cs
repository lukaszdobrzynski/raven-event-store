using System;
using Raven.EventStore;

namespace RavenEventStoreTestModels.Events;

public class UserActivatedEvent : Event
{
    public override string Name => nameof(UserActivatedEvent);
    public string Status { get; init; }
    
    public static UserActivatedEvent Create => new()
    {
        Status = "ACTIVATED", 
        EventId = Guid.NewGuid(), 
        Timestamp = DateTime.UtcNow
    };
}