namespace SimpleDomain.EventStore.Persistence;

using global::EventStore.Client;

internal class EventStoreClientFactory(string connectionString)
{
    public EventStoreClient Create() => new(EventStoreClientSettings.Create(connectionString));
}