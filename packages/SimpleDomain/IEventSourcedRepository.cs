namespace SimpleDomain;

/// <summary>
/// The event sourced repository interface for aggregate roots.
/// </summary>
public interface IEventSourcedRepository
{
    /// <summary>
    /// Gets an aggregate root by its id.
    /// </summary>
    /// <param name="aggregateId">The aggregate root id.</param>
    /// <typeparam name="TAggregateRoot">The type of the aggregate root.</typeparam>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    Task<TAggregateRoot> GetById<TAggregateRoot>(string aggregateId, CancellationToken cancellationToken = default)
        where TAggregateRoot : EventSourcedAggregateRoot;

    /// <summary>
    /// Persists a new or modified aggregate root.
    /// </summary>
    /// <param name="aggregateRoot">The aggregate root.</param>
    /// <typeparam name="TAggregateRoot">The type of the aggregate root.</typeparam>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    Task Save<TAggregateRoot>(
        TAggregateRoot aggregateRoot,
        CancellationToken cancellationToken = default)
        where TAggregateRoot : EventSourcedAggregateRoot;

    /// <summary>
    /// Persists a new or modified aggregate root.
    /// </summary>
    /// <param name="aggregateRoot">The aggregate root.</param>
    /// <param name="headers">A list of arbitrary headers which serve as meta information.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <typeparam name="TAggregateRoot">The type of the aggregate root.</typeparam>
    Task Save<TAggregateRoot>(
        TAggregateRoot aggregateRoot,
        IDictionary<string, object> headers,
        CancellationToken cancellationToken = default)
        where TAggregateRoot : EventSourcedAggregateRoot;

    /// <summary>
    /// Persists a new or modified aggregate root.
    /// </summary>
    /// <param name="aggregateRoot">The aggregate root.</param>
    /// <typeparam name="TAggregateRoot">The type of the aggregate root.</typeparam>
    /// <param name="publishEvent">The callback to publish a single event.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    Task Save<TAggregateRoot>(
        TAggregateRoot aggregateRoot,
        Func<object, Task> publishEvent,
        CancellationToken cancellationToken = default)
        where TAggregateRoot : EventSourcedAggregateRoot;

    /// <summary>
    /// Persists a new of modified aggregate root.
    /// </summary>
    /// <param name="aggregateRoot">The aggregate root.</param>
    /// <param name="headers">A list of arbitrary headers which serve as meta information.</param>
    /// <param name="publishEvent">The callback to publish a single event.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <typeparam name="TAggregateRoot">The type of the aggregate root.</typeparam>
    Task Save<TAggregateRoot>(
        TAggregateRoot aggregateRoot,
        IDictionary<string, object> headers,
        Func<object, Task> publishEvent,
        CancellationToken cancellationToken = default)
        where TAggregateRoot : EventSourcedAggregateRoot;
}