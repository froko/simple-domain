namespace SimpleDomain;

/// <summary>
/// Base class for all event sourced aggregate roots.
/// </summary>
public abstract class EventSourcedAggregateRoot : AggregateRoot, INeedVersion
{
    private readonly Dictionary<Type, Action<object>> transitions = [];

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
    /// <param name="events">The list of events.</param>
    public async Task LoadFromEventHistory(IAsyncEnumerable<object> events)
    {
        await foreach (var @event in events)
            this.ApplyEvent(@event, false);
    }

    /// <summary>
    /// Registers the state changing transition action.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    /// <param name="transition">The method that is called when an event is applied.</param>
    protected void RegisterTransition<TEvent>(Action<TEvent> transition)
        where TEvent : class =>
        this.transitions.Add(typeof(TEvent), @event => transition((@event as TEvent)!));

    /// <summary>
    /// Applies a change expressed as event
    /// </summary>
    /// <param name="event">The event</param>
    protected override void ApplyEvent(object @event) => this.ApplyEvent(@event, true);

    private void ApplyEvent(object @event, bool isNew)
    {
        this.Version++;
        this.DoTransition(@event);

        if (!isNew)
            return;

        base.ApplyEvent(new VersionableEvent(@event, this.Version, DateTimeOffset.UtcNow));
    }

    private void DoTransition(object @event)
    {
        var eventType = @event.GetType();
        if (this.transitions.TryGetValue(eventType, out var doTransition)) doTransition(@event);
    }
}