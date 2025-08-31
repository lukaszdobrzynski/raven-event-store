[![ci-pipeline](https://github.com/lukaszdobrzynski/raven-event-store/actions/workflows/ci-pipeline.yml/badge.svg)](https://github.com/lukaszdobrzynski/raven-event-store/actions/workflows/ci-pipeline.yml)

# Raven Event Store

**Raven Event Store** is a lightweight `.NET` class library built on top of the RavenDB client. It provides a clean and efficient way to store and retrieve application state as a stream of events, embracing the principles of event sourcing.

### 🚀 Features

- 🗃️ Leverages **RavenDB’s high-performance document database**
- 📜 Captures changes in your application as **immutable business facts**
- 🧩 Simplifies event persistence and retrieval with **minimal boilerplate**
- ⏳ Supports **event versioning**, **stream slicing**, and **global event logging**
- 🏗️ Rebuilds **aggregates** from stored event streams
- 🔌 Allows configuration of **multiple stores** within a single application

### 🧠 Why Event Sourcing?

Instead of persisting just the current state, event sourcing captures a complete history of all changes to your data. This approach offers powerful benefits:

- 🔍 Full auditability
- 🔄 Reproducible state
- 🧩 Clear separation of concerns
- 🛠️ Easier debugging and troubleshooting
- 🔑 Natural fit for CQRS

### 🦅 Built For RavenDB

By leveraging RavenDB under the hood, Raven Event Store gives an application a robust and production-ready foundation for event storage, including:

- ⚡ Optimistic concurrency control
- 📦 Batch processing
- ✅ ACID guarantees
- 🔔 Live subscriptions
- 🧠 Advanced indexing
- 🗃️ Data archival
- 🌐 Support for distributed systems and high availability

### 📦 Configuration And Sample Usage

```csharp
DocumentStore.AddEventStore(options =>
{
    options.DatabaseName = "database-name";
    options.Aggregates.Register(registry =>
    {
        registry.Add<User>();
        registry.Add<Product>();
        registry.Add<Cart>();
    });
    options.UseGlobalStreamLogging = true;
});

var eventStore = DocumentStore.GetEventStore("database-name");

var stream = await eventStore.CreateStreamAndStoreAsync<UserStream>(
    UserRegisteredEvent.Create(username: "event-sorcerer", email: "john@event-sorcerer.com"));

await eventStore.AppendAndStoreAsync<UserStream>(stream.Id, UserVerifiedEvent.Create, UserActivatedEvent.Create);

await eventStore.SliceStreamAndStoreAsync<UserStream>(stream.Id, "next-slice-id", UserRoleChangedEvent.Create("SUPER-ADMIN"));
```
### 🪶 License

MIT – free to use, contribute, and extend.

 


