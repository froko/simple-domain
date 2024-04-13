namespace SimpleDomain;

using Microsoft.Extensions.DependencyInjection;

using SimpleDomain.EventStore;
using SimpleDomain.EventStore.Persistence;

/// <summary>
/// The simple domain configurator.
/// </summary>
/// <param name="collection">Dependency injection for <see cref="IServiceCollection"/>.</param>
internal class SimpleDomainConfigurator(IServiceCollection collection)
    : IConfigureSimpleDomain, IConfigureEventStore, IHaveEventStoreConfiguration
{
    private SnapshotStrategy globalSnapshotStrategy = new(100);
    private readonly List<SnapshotStrategy> snapshotStrategies = [];
    private readonly Dictionary<string, object> configurationItems = [];

    /// <summary>
    /// Completes the configuration by adding the <see cref="IHaveEventStoreConfiguration"/>
    /// to the dependency injection container.
    /// </summary>
    public void Complete() => collection.AddSingleton<IHaveEventStoreConfiguration>(this);

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
    public void AddSingleton<TService, TImplementation>()
        where TService : class
        where TImplementation : class, TService =>
        collection.AddSingleton<TService, TImplementation>();

    public void AddInstance<TService>(TService instance)
        where TService : class =>
        collection.AddSingleton(instance);

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
}
