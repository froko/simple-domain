namespace SimpleDomain.EventStore;

/// <summary>
/// The event store configuration holder interface.
/// </summary>
public interface IHaveEventStoreConfiguration
{
    /// <summary>
    /// Gets the snapshot strategy for a certain type of aggregate root
    /// or the global snapshot strategy if no specific one is defined.
    /// </summary>
    /// <typeparam name="TAggregateRoot">The type of the aggregate root.</typeparam>
    /// <returns>A snapshot strategy.</returns>
    SnapshotStrategy GetSnapshotStrategy<TAggregateRoot>() where TAggregateRoot : EventSourcedAggregateRoot;

    /// <summary>
    /// Gets a typed configuration item by its key.
    /// </summary>
    /// <typeparam name="T">The type of the configuration item</typeparam>
    /// <param name="key">The key</param>
    /// <returns>A typed configuration item</returns>
    T Get<T>(string key);
}