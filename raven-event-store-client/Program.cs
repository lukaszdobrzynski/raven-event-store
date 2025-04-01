using Raven.EventStore;
using Raven.EventStore.Client;
using Raven.EventStore.Client.Events;

var store = DocumentStoreHolder.Store;

var eventStore = store.GetEventStore();

var registered = UserRegisteredEvent.Create("event-sorcerer", "lukasz@event-driven.com");
var verified = UserVerifiedEvent.Create;
var activated = UserActivatedEvent.Create;
var changedEmail = UserChangedEmailEvent.Create("lukasz@event-driven.io");
var deactivated = UserDeactivatedEvent.Create;

var stream = await eventStore.CreateStreamAsync<UserStream>(registered);
await eventStore.AppendAsync<UserStream>(stream.Id, verified, activated);
await eventStore.AppendAsync<UserStream>(stream.Id, changedEmail);
await eventStore.AppendAsync<UserStream>(stream.Id, deactivated);
