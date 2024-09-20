namespace SimpleDomain;

using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// The base class for all aggregate roots.
/// </summary>
public abstract class AggregateRoot
{
    private readonly List<object> uncommittedEvents = [];

    /// <summary>
    /// Gets all uncommitted events.
    /// </summary>
    [NotMapped]
    public IEnumerable<object> UncommittedEvents => this.uncommittedEvents;

    /// <summary>
    /// Commits all uncommitted events.
    /// </summary>
    public void CommitEvents() => this.uncommittedEvents.Clear();

    /// <summary>
    /// Applies an event to the aggregate root.
    /// </summary>
    /// <param name="event">The event.</param>
    protected virtual void ApplyEvent(object @event) => this.uncommittedEvents.Add(@event);
}