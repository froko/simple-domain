namespace SimpleDomain.EventStore.Persistence;

using System.Text;
using global::EventStore.Client;
using Newtonsoft.Json;

internal static class EventStoreExtensions
{
    public static byte[] AsByteArray(this object @event) => Serialize(@event);

    public static byte[] AsByteArray(this ISnapshot snapshot) => Serialize(snapshot);

    public static byte[] AsByteArray(this IDictionary<string, object> eventHeaders) => Serialize(eventHeaders);

    public static EventData SerializeInnerEvent(
        this VersionableEvent @event,
        TypeNameStrategy typeNameStrategy,
        IDictionary<string, object> headers
    ) => EventDataBuilder.Initialize(@event.InnerEvent, typeNameStrategy).AddHeaders(headers).Build();

    public static EventData SerializeSnapshot(this ISnapshot snapshot, TypeNameStrategy typeNameStrategy) =>
        SnapshotDataBuilder.Initialize(snapshot, typeNameStrategy).Build();

    public static IAsyncEnumerable<object> DeserializeAsEvents(this EventStoreClient.ReadStreamResult stream, int skip)
        => stream.Skip(skip).Select(DeserializeAsEvent);

    public static ISnapshot DeserializeAsSnapshot(this ResolvedEvent resolvedEvent)
    {
        var snapshotData = resolvedEvent.Event.Data;
        var typeName = resolvedEvent.Event.EventType;
        return snapshotData.Deserialize<ISnapshot>(typeName);
    }

    public static object DeserializeAsEvent(this ResolvedEvent resolvedEvent)
    {
        var eventData = resolvedEvent.Event.Data;
        var typeName = resolvedEvent.Event.EventType;
        return eventData.Deserialize<object>(typeName);
    }

    private static byte[] Serialize(object obj) => Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(obj));

    private static T Deserialize<T>(this ReadOnlyMemory<byte> data, string typeName)
    {
        var jsonString = Encoding.UTF8.GetString(data.ToArray());
        return (T)JsonConvert.DeserializeObject(jsonString, Type.GetType(typeName)!)!;
    }
}