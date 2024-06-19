namespace SimpleDomain;

using EventStore;
using EventStore.Persistence;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// The EventStore aware SimpleDomain configurator.
/// </summary>
/// <param name="collection"></param>
internal sealed class SimpleDomainEventStoreConfigurator(IServiceCollection collection)
    : SimpleDomainConfigurator(collection),
        IConfigureSimpleDomainWithEventStore
{
    /// <inheritdoc />
    public IConfigureEventStore UseEventStore(string connectionString, TypeNameStrategy? typeNameStrategy = null)
    {
        this.AddInstance(typeNameStrategy ?? TypeNameStrategy.Loose);
        this.AddInstance(new EventStoreClientFactory(connectionString));
        this.AddSingleton<IEventStore, EventStoreStore>();
        this.AddSingleton<IEventSourcedRepository, EventStoreRepository>();
        return this;
    }
}