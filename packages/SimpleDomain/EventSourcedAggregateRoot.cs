namespace SimpleDomain;

/// <summary>
/// Base class for all event sourced aggregate roots.
/// </summary>
public abstract class EventSourcedAggregateRoot : AggregateRoot, INeedVersion
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EventSourcedAggregateRoot" /> class.
    /// </summary>
    protected EventSourcedAggregateRoot()
    {
        this.Id = string.Empty;
        this.Version = -1;
    }

    /// <summary>
    /// Gets the id of this aggregate root.
    /// </summary>
    public string Id { get; protected set; }

    /// <inheritdoc />
    public int Version { get; protected set; }

    /// <summary>
    /// Creates a snapshot of this aggregate root.
    /// </summary>
    /// <returns></returns>
    public virtual ISnapshot CreateSnapshot() => null!;

    /// <summary>
    /// Builds up the aggregate root from a snapshot.
    /// </summary>
    /// <param name="snapshot"></param>
    public virtual void LoadFromSnapshot(ISnapshot snapshot) { }

    /// <summary>
    /// Builds up the aggregate root from a list of events.
    /// </summary>
    /// <param name="eventHistory">The history as a list of events.</param>
    public void LoadFromEventHistory(EventHistory eventHistory)
    {
        foreach (var @event in eventHistory) this.ApplyEvent(@event, false);
    }

    /// <summary>
    /// Applies a change expressed as event
    /// </summary>
    /// <param name="event">The event</param>
    protected override void ApplyEvent(IEvent @event) => this.ApplyEvent(@event, true);

    /// <summary>
    /// Executes a state transition on a derived aggregate root
    /// </summary>
    /// <param name="event">The event</param>
    protected abstract void DoTransition(IEvent @event);

    private void ApplyEvent(IEvent @event, bool isNew)
    {
        this.Version++;
        this.DoTransition(@event);

        if (!isNew) return;

        base.ApplyEvent(new VersionableEvent(@event, this.Version, DateTimeOffset.UtcNow));
    }
}
