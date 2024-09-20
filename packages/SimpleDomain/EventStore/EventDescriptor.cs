namespace SimpleDomain.EventStore;

using System.Collections.ObjectModel;

/// <summary>
/// Describes an event enriched with further attributes.
/// </summary>
/// <param name="AggregateType">The full CLR name of the aggregate root.</param>
/// <param name="AggregateId">The id of the aggregate root.</param>
/// <param name="Version">The version of the event.</param>
/// <param name="Timestamp">The timestamp of the event.</param>
/// <param name="EventType">The full CLR name of the event.</param>
/// <param name="Event">The event itself.</param>
/// <param name="Headers">A list of arbitrary headers.</param>
public record EventDescriptor(
    string AggregateType,
    string AggregateId,
    int Version,
    DateTimeOffset Timestamp,
    string EventType,
    object Event,
    IReadOnlyDictionary<string, object> Headers)
{
    /// <summary>
    /// Creates a new event descriptor.
    /// </summary>
    /// <param name="aggregateId">The id of the aggregate root.</param>
    /// <param name="versionableEvent">The versionable event.</param>
    /// <param name="headers">A list of arbitrary headers.</param>
    /// <typeparam name="TAggregateRoot">The type of the aggregate root.</typeparam>
    /// <returns>A new instance of <see cref="EventDescriptor" />.</returns>
    public static EventDescriptor From<TAggregateRoot>(
        string aggregateId,
        VersionableEvent versionableEvent,
        IDictionary<string, object> headers)
        where TAggregateRoot : EventSourcedAggregateRoot
        => new(
            typeof(TAggregateRoot).FullName!,
            aggregateId,
            versionableEvent.Version,
            versionableEvent.Timestamp,
            versionableEvent.InnerEvent.GetType().FullName!,
            versionableEvent.InnerEvent,
            new ReadOnlyDictionary<string, object>(headers));
}