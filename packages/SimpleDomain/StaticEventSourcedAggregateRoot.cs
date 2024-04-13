namespace SimpleDomain;

/// <summary>
/// Base class for all statically event sourced aggregate roots.
/// </summary>
public abstract class StaticEventSourcedAggregateRoot : EventSourcedAggregateRoot
{
    private readonly Dictionary<Type, Action<IEvent>> transitions = [];

    /// <summary>
    /// Registers the state changing transition action.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    /// <param name="transition">The method that is called when an event is applied.</param>
    protected void RegisterTransition<TEvent>(Action<TEvent> transition)
        where TEvent : class, IEvent =>
        this.transitions.Add(typeof(TEvent), @event => transition((@event as TEvent)!));

    /// <inheritdoc />
    protected override void DoTransition(IEvent @event)
    {
        var eventType = @event.GetType();
        if (this.transitions.TryGetValue(eventType, out var doTransition)) doTransition(@event);
    }
}
