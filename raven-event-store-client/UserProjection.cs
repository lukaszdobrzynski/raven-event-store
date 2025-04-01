using System;
using Raven.EventStore.Client.Events;

namespace Raven.EventStore.Client;

public class UserProjection : Projection<UserStream>
{
    public string Username { get; set; }
    public string Email { get; set; }
    public string VerificationStatus { get; set; }
    public string ActivationStatus { get; set; }
    public DateTime RegisteredAt { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public DateTime? ActivationStatusChangedAt { get; set; }

    protected override void Project(UserStream userStream)
    {
        var events = userStream.Events;
        
        foreach (var @event in events)
        {
            switch (@event)
            {
                case UserRegisteredEvent userRegistered:
                    Apply(userRegistered);
                    break;
                case UserVerifiedEvent userVerified:
                    Apply(userVerified);
                    break;
                case UserActivatedEvent userActivated:
                    Apply(userActivated);
                    break;
                case UserDeactivatedEvent userDeactivated:
                    Apply(userDeactivated);
                    break;
                case UserChangedEmailEvent userChangedEmail:
                    Apply(userChangedEmail);
                    break;
                default:
                    throw new ArgumentException($"Unknown event type: {@event.GetType().Name}");
            }    
        }
    }

    protected override string GetId(UserStream stream)
    {
        return stream.Id + "/" + $"{nameof(UserProjection)}";
    }

    private void Apply(UserRegisteredEvent @event)
    {
        Username = @event.Username;
        Email = @event.Email;
        VerificationStatus = @event.VerificationStatus;
        ActivationStatus = @event.ActivationStatus;
        RegisteredAt = @event.RegisteredAt;
        ActivationStatus = @event.ActivationStatus;
    }

    private void Apply(UserActivatedEvent @event)
    {
        ActivationStatus = @event.ActivationStatus;
        ActivationStatusChangedAt = @event.ActivatedAt;
    }

    private void Apply(UserDeactivatedEvent @event)
    {
        ActivationStatus = @event.ActivationStatus;
        ActivationStatusChangedAt = @event.DeactivatedAt;
    }

    private void Apply(UserVerifiedEvent @event)
    {
        VerificationStatus = @event.VerificationStatus;
        VerifiedAt = @event.VerifiedAt;
    }

    private void Apply(UserChangedEmailEvent @event)
    {
        Email = @event.Email;
    }
}