namespace SimpleDomain.EventStore;

/// <summary>
/// The abstract base class of an event stream.
/// </summary>
/// <typeparam name="TAggregateRoot">The type of the aggregate root.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="EventStream{TAggregateRoot}" /> class.
/// </remarks>
/// <param name="aggregateId">The id of the aggregate root.</param>
public abstract class EventStream<TAggregateRoot>(string aggregateId) : IEventStream
    where TAggregateRoot : EventSourcedAggregateRoot
{
    /// <summary>
    /// Gets the full CLR name of the aggregate root.
    /// </summary>
    protected string AggregateType { get; private set; } = typeof(TAggregateRoot).FullName!;

    /// <summary>
    /// Gets the id of the aggregate root.
    /// </summary>
    protected string AggregateId { get; private set; } = aggregateId;

    /// <inheritdoc />
    public abstract Task<IEventStream> Open(CancellationToken cancellationToken);

    /// <inheritdoc />
    public async Task Append(
        IReadOnlyCollection<VersionableEvent> events,
        int expectedVersion,
        IDictionary<string, object> headers,
        CancellationToken cancellationToken
    )
    {
        var originalVersion = expectedVersion - events.Count;
        this.CheckForConcurrencyProblems(originalVersion);
        foreach (var @event in events)
            await this.Save(@event, headers, cancellationToken);
    }

    /// <inheritdoc />
    public abstract Task SaveSnapshot(ISnapshot snapshot, CancellationToken cancellationToken);

    /// <inheritdoc />
    public Task<IAsyncEnumerable<object>> Replay(CancellationToken cancellationToken) =>
        this.Replay(0, cancellationToken);

    /// <inheritdoc />
    public Task<IAsyncEnumerable<object>> ReplayFromSnapshot(ISnapshot snapshot, CancellationToken cancellationToken) =>
        this.Replay(snapshot.Version + 1, cancellationToken);

    /// <inheritdoc />
    public abstract Task<bool> HasSnapshot(CancellationToken cancellationToken);

    /// <inheritdoc />
    public abstract Task<ISnapshot> GetLatestSnapshot(CancellationToken cancellationToken);

    /// <inheritdoc />
    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Persists a single event.
    /// </summary>
    /// <param name="event">The versionable event.</param>
    /// <param name="headers">A list of arbitrary headers.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    protected abstract Task Save(
        VersionableEvent @event,
        IDictionary<string, object> headers,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Replays all events of an aggregate root between two given version boundaries.
    /// </summary>
    /// <param name="fromVersion">The lower version boundary (inclusive).</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>An async stream of events.</returns>
    protected abstract Task<IAsyncEnumerable<object>> Replay(int fromVersion, CancellationToken cancellationToken);

    /// <summary>
    /// When overridden, checks for concurrency problems
    /// by comparing the last persisted version with the given original version.
    /// </summary>
    /// <param name="originalVersion">
    /// The original version, calculated as expected version minus the number of events that are
    /// going to be persisted.
    /// </param>
    protected virtual void CheckForConcurrencyProblems(int originalVersion) { }

    protected virtual void Dispose(bool disposing) { }
}