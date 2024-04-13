namespace SimpleDomain;

using SimpleDomain.Common;

/// <summary>
/// Base class for all dynamic event sourced aggregate roots.
/// </summary>
public abstract class DynamicEventSourcedAggregateRoot : EventSourcedAggregateRoot
{
    /// <inheritdoc />
    protected override void DoTransition(IEvent @event) => this.AsDynamic().Apply(@event);
}
