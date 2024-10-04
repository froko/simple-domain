namespace SimpleDomain.EventStore.Persistence;

using global::EventStore.Client;

internal class EventDataBuilder
{
    private readonly object @event;
    private readonly IDictionary<string, object> eventHeaders = new Dictionary<string, object>();
    private readonly Uuid eventId;
    private readonly string eventName;

    private EventDataBuilder(object @event, TypeNameStrategy typeNameStrategy)
    {
        this.@event = @event;
        this.eventId = Uuid.NewUuid();
        this.eventName = typeNameStrategy.GetTypeName(@event);
    }

    public static EventDataBuilder Initialize(object @event, TypeNameStrategy typeNameStrategy) =>
        new(@event, typeNameStrategy);

    public EventDataBuilder AddHeaders(IDictionary<string, object> headers)
    {
        foreach (var header in headers.Where(header => !this.eventHeaders.Contains(header)))
            this.eventHeaders.Add(header.Key, header.Value);

        return this;
    }

    public EventData Build()
        => new(this.eventId, this.eventName, this.@event.AsByteArray(), this.eventHeaders.AsByteArray());
}