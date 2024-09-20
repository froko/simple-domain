namespace SimpleDomain.EventStore;

/// <summary>
/// Describes a snapshot enriched with further attributes.
/// </summary>
/// <param name="AggregateType">The full CLR name of the aggregate root.</param>
/// <param name="AggregateId">The id of the aggregate root.</param>
/// <param name="Version">The version of the snapshot.</param>
/// <param name="Timestamp">The timestamp of the snapshot.</param>
/// <param name="SnapshotType">The full CLR name of the snapshot.</param>
/// <param name="Snapshot">The snapshot itself.</param>
public record SnapshotDescriptor(
    string AggregateType,
    string AggregateId,
    int Version,
    DateTimeOffset Timestamp,
    string SnapshotType,
    ISnapshot Snapshot)
{
    /// <summary>
    /// Creates a new snapshot descriptor.
    /// </summary>
    /// <param name="aggregateId">The id of the aggregate root.</param>
    /// <param name="snapshot">The snapshot.</param>
    /// <typeparam name="TAggregateRoot">The type of the aggregate root.</typeparam>
    /// <returns>A new instance of <see cref="SnapshotDescriptor" />.</returns>
    public static SnapshotDescriptor From<TAggregateRoot>(
        string aggregateId,
        ISnapshot snapshot)
        where TAggregateRoot : EventSourcedAggregateRoot
        => new(
            typeof(TAggregateRoot).FullName!,
            aggregateId,
            snapshot.Version,
            snapshot.Timestamp,
            snapshot.GetType().FullName!,
            snapshot);
}