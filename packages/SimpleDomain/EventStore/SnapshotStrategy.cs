namespace SimpleDomain.EventStore;

/// <summary>
/// The snapshot strategy which defines when to take a snapshot of an aggregate root.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="SnapshotStrategy" /> class.
/// </remarks>
/// <param name="threshold">The version threshold on which a snapshot is taken.</param>
/// <param name="aggregateType">The type of the aggregate root.</param>
public class SnapshotStrategy(int threshold, Type aggregateType)
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SnapshotStrategy" /> class.
    /// </summary>
    /// <param name="threshold">The version threshold on which a snapshot is taken.</param>
    public SnapshotStrategy(int threshold) : this(threshold, typeof(EventSourcedAggregateRoot)) { }

    /// <summary>
    /// Returns true if the given type of the aggregate root
    /// is assignable to the internal type and the internal type itself and
    /// is not the type of the <see cref="EventSourcedAggregateRoot" /> base class.
    /// </summary>
    /// <typeparam name="TAggregateRoot">The type of the aggregate root.</typeparam>
    /// <returns><c>True</c> if the given type of the aggregate root is assignable to this or <c>false</c> if not.</returns>
    public bool AppliesToThisAggregateRoot<TAggregateRoot>() where TAggregateRoot : EventSourcedAggregateRoot =>
        this.IsAssignableToMe<TAggregateRoot>() && this.IamNotTheAggregateRootBaseType();

    /// <summary>
    /// Checks if the given aggregate root needs to be snapshotted due to its version
    /// compared with the threshold.
    /// </summary>
    /// <typeparam name="TAggregateRoot">The type of the aggregate root.</typeparam>
    /// <param name="aggregateRoot">The aggregate root.</param>
    /// <returns>
    /// <c>True</c> if the aggregate root version is equal or a multiple of the threshold value or <c>false</c> if
    /// not.
    /// </returns>
    public bool NeedsSnapshot<TAggregateRoot>(TAggregateRoot aggregateRoot)
        where TAggregateRoot : EventSourcedAggregateRoot =>
        this.IsAssignableToMe<TAggregateRoot>() && this.IsMultipleOfThreshold(aggregateRoot.Version);

    private bool IsAssignableToMe<TAggregateRoot>() where TAggregateRoot : EventSourcedAggregateRoot =>
        aggregateType.IsAssignableFrom(typeof(TAggregateRoot));

    private bool IamNotTheAggregateRootBaseType() => aggregateType != typeof(EventSourcedAggregateRoot);

    private bool IsMultipleOfThreshold(int version) => version != 0 && version % threshold == 0;
}