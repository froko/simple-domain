namespace SimpleDomain;

using System.Collections;

/// <summary>
/// Represents the history of events used to bring
/// an Aggregate Root in its desired state.
/// </summary>
public class EventHistory : IEnumerable<IEvent>
{
    private readonly IReadOnlyCollection<IEvent> events;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventHistory" /> class.
    /// </summary>
    /// <param name="events">All events that have been applied to the Aggregate Root in the past.</param>
    public EventHistory(IEnumerable<IEvent> events) => this.events = new List<IEvent>(events);

    /// <summary>
    /// Gets a value indicating whether no events have been found/loaded.
    /// </summary>
    public bool IsEmpty => this.events.Count == 0;

    /// <inheritdoc />
    public IEnumerator<IEvent> GetEnumerator() => this.events.GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => this.events.GetEnumerator();

    /// <summary>
    /// Factory method to create a new instance of <see cref="EventHistory" />.
    /// </summary>
    /// <param name="events">All events that have been applied to the Aggregate Root in the past.</param>
    /// <returns>A new instance of <see cref="EventHistory" />.</returns>
    public static EventHistory Create(params IEvent[] events) => new(events);
}
