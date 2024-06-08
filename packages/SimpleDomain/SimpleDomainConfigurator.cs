namespace SimpleDomain;

using EventStore;
using EventStore.Persistence;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// The SimpleDomain configurator.
/// </summary>
/// <param name="collection">Dependency injection for <see cref="IServiceCollection" />.</param>
public class SimpleDomainConfigurator(IServiceCollection collection)
    : IConfigureSimpleDomain, IConfigureEventStore, IHaveEventStoreConfiguration
{
    private readonly Dictionary<string, object> configurationItems = [];
    private readonly List<SnapshotStrategy> snapshotStrategies = [];
    private SnapshotStrategy globalSnapshotStrategy = new(100);

    /// <inheritdoc />
    public IConfigureEventStore WithGlobalSnapshotStrategy(int threshold)
    {
        this.globalSnapshotStrategy = new SnapshotStrategy(threshold);
        return this;
    }

    /// <inheritdoc />
    public IConfigureEventStore WithSnapshotStrategyFor<TAggregateRoot>(int threshold)
        where TAggregateRoot : EventSourcedAggregateRoot
    {
        var existingSnapshotStrategy = this.snapshotStrategies
            .FirstOrDefault(s => s.AppliesToThisAggregateRoot<TAggregateRoot>());
        if (existingSnapshotStrategy != null) this.snapshotStrategies.Remove(existingSnapshotStrategy);

        this.snapshotStrategies.Add(new SnapshotStrategy(threshold, typeof(TAggregateRoot)));
        return this;
    }

    /// <inheritdoc />
    public IConfigureEventStore UseInMemoryEventStore()
    {
        this.AddConfigurationItem(InMemoryEventStore.EventDescriptors, new List<EventDescriptor>());
        this.AddConfigurationItem(InMemoryEventStore.SnapshotDescriptors, new List<SnapshotDescriptor>());
        collection.AddTransient<IEventStore, InMemoryEventStore>();
        collection.AddTransient<IEventSourcedRepository, EventStoreRepository>();
        return this;
    }

    /// <inheritdoc />
    public void AddConfigurationItem(string key, object value) => this.configurationItems.TryAdd(key, value);

    /// <inheritdoc />
    public void AddTransient<TService, TImplementation>()
        where TService : class
        where TImplementation : class, TService =>
        collection.AddTransient<TService, TImplementation>();

    /// <inheritdoc />
    public SnapshotStrategy GetSnapshotStrategy<TAggregateRoot>()
        where TAggregateRoot : EventSourcedAggregateRoot
    {
        var snapshotStrategy = this.snapshotStrategies
            .FirstOrDefault(s => s.AppliesToThisAggregateRoot<TAggregateRoot>());
        return snapshotStrategy ?? this.globalSnapshotStrategy;
    }

    /// <inheritdoc />
    public T Get<T>(string key) =>
        !this.configurationItems.TryGetValue(key, out var value)
            ? throw new KeyNotFoundException()
            : value is not T typedValue
                ? throw new InvalidCastException()
                : typedValue;

    /// <summary>
    /// Completes the configuration by adding <see cref="IHaveEventStoreConfiguration" />
    /// to the dependency injection container.
    /// </summary>
    public void Complete() => collection.AddSingleton<IHaveEventStoreConfiguration>(this);

    protected void AddSingleton<TService, TImplementation>()
        where TService : class
        where TImplementation : class, TService =>
        collection.AddSingleton<TService, TImplementation>();

    protected void AddInstance<TService>(TService instance)
        where TService : class =>
        collection.AddSingleton(instance);
}