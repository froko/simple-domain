namespace SimpleDomain.EventStore;

/// <summary>
/// The event store configuration interface.
/// </summary>
public interface IConfigureEventStore
{
    /// <summary>
    /// Defines a global snapshot strategy.
    /// </summary>
    /// <param name="threshold">The version threshold on which a snapshot is taken.</param>
    /// <returns>An instance of <see cref="IConfigureEventStore" /> since this is a fluent configuration.</returns>
    IConfigureEventStore WithGlobalSnapshotStrategy(int threshold);

    /// <summary>
    /// Defines a snapshot strategy for a certain type of aggregate root.
    /// </summary>
    /// <param name="threshold">The version threshold on which a snapshot is taken.</param>
    /// <typeparam name="TAggregateRoot">The type of the aggregate root.</typeparam>
    /// <returns>An instance of <see cref="IConfigureEventStore" /> since this is a fluent configuration.</returns>
    IConfigureEventStore WithSnapshotStrategyFor<TAggregateRoot>(int threshold)
        where TAggregateRoot : EventSourcedAggregateRoot;
}