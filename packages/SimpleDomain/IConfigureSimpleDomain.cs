namespace SimpleDomain;

using EventStore;

/// <summary>
/// The SimpleDomain configuration interface.
/// </summary>
public interface IConfigureSimpleDomain
{
    /// <summary>
    /// Use the in-memory event store.
    /// </summary>
    /// <returns>An instance of <see cref="IConfigureEventStore" /> since this is a fluent configuration.</returns>
    IConfigureEventStore UseInMemoryEventStore();

    /// <summary>
    /// Adds a configuration item.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The item.</param>
    void AddConfigurationItem(string key, object value);

    /// <summary>
    /// Adds a transient service to the dependency injection container.
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    /// <typeparam name="TImplementation"></typeparam>
    void AddTransient<TService, TImplementation>()
        where TService : class
        where TImplementation : class, TService;
}