namespace SimpleDomain.EventStore;

/// <summary>
/// The event stream interface.
/// </summary>
public interface IEventStream : IDisposable
{
    /// <summary>
    /// Opens the connection to the persistence engine.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>The event stream itself.</returns>
    Task<IEventStream> Open(CancellationToken cancellationToken);

    /// <summary>
    /// Appends a list of events.
    /// </summary>
    /// <param name="events">A list of versionable events.</param>
    /// <param name="expectedVersion">The actual version of the aggregate root.</param>
    /// <param name="headers">A list of arbitrary headers.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    Task Append(
        IReadOnlyCollection<VersionableEvent> events,
        int expectedVersion,
        IDictionary<string, object> headers,
        CancellationToken cancellationToken);

    /// <summary>
    /// Persists a snapshot.
    /// </summary>
    /// <param name="snapshot">The snapshot.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    Task SaveSnapshot(ISnapshot snapshot, CancellationToken cancellationToken);

    /// <summary>
    /// Replays all events of an aggregate root.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>An async stream of events.</returns>
    Task<IAsyncEnumerable<object>> Replay(CancellationToken cancellationToken);

    /// <summary>
    /// Replays all events of an aggregate root from a given snapshot.
    /// </summary>
    /// <param name="snapshot">The snapshot.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>An async stream of events.</returns>
    Task<IAsyncEnumerable<object>> ReplayFromSnapshot(ISnapshot snapshot, CancellationToken cancellationToken);

    /// <summary>
    /// Returns the fact that there exists at least one snapshot.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns><c>True</c> if this stream has a snapshot or <c>false</c> if not.</returns>
    Task<bool> HasSnapshot(CancellationToken cancellationToken);

    /// <summary>
    /// Gets the latest snapshot.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>The latest snapshot.</returns>
    Task<ISnapshot> GetLatestSnapshot(CancellationToken cancellationToken);
}