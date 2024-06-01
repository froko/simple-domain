namespace SimpleDomain.EventStore;

/// <summary>
/// The event store interface.
/// </summary>
public interface IEventStore
{
    /// <summary>
    /// Returns an event stream for an aggregate root with a given id.
    /// </summary>
    /// <param name="aggregateId">The id of the aggregate root.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <typeparam name="TAggregateRoot">The type of the aggregate root.</typeparam>
    /// <returns>An event stream.</returns>
    Task<IEventStream> OpenStream<TAggregateRoot>(string aggregateId, CancellationToken cancellationToken)
        where TAggregateRoot : EventSourcedAggregateRoot;

    /// <summary>
    /// Replays all events which are stored in the event store.
    /// </summary>
    /// <param name="publishEvent">The callback to publish a single event.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    Task ReplayAll(
        Func<object, CancellationToken, Task> publishEvent,
        CancellationToken cancellationToken);
}