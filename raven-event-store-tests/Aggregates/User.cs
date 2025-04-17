using Raven.EventStore.Tests.Events;
using Raven.EventStore.Tests.Streams;

namespace Raven.EventStore.Tests.Aggregates;

public class User : Aggregate<UserStream>
{
    public string Username { get; set; }
    public string Email { get; set; }
    public string Status { get; set; }
    public string Role { get; set; }
    
    protected override void Build(UserStream stream)
    {
        foreach (var @event in stream.Events)
        {
            switch (@event)
            {
                case UserRegisteredEvent e:
                    Apply(e);
                    break;
                case UserVerifiedEvent e:
                    Apply(e);
                    break;
                case UserActivatedEvent e:
                    Apply(e);
                    break;
                case UserChangedEmailEvent e:
                    Apply(e);
                    break;
                case UserRoleChangedEvent e:
                    Apply(e);
                    break;
                case UserDeactivatedEvent e:
                    Apply(e);
                    break;
            }
        }
    }
    
    private void Apply(UserRegisteredEvent @event)
    {
        Username = @event.Username;
        Email = @event.Email;
        Status = @event.Status;
        Role = @event.Role;
    }

    private void Apply(UserVerifiedEvent @event)
    {
        Status = @event.Status;
    }
    
    private void Apply(UserActivatedEvent @event)
    {
        Status = @event.Status;
    }

    private void Apply(UserDeactivatedEvent @event)
    {
        Status = @event.Status;
    }

    private void Apply(UserChangedEmailEvent @event)
    {
        Email = @event.Email;
    }

    private void Apply(UserRoleChangedEvent @event)
    {
        Role = @event.Role;
    }
}