namespace SimpleDomain.EventStore.Persistence;

using System.Globalization;
using FluentAssertions;
using global::EventStore.Client;
using TestDoubles;
using Xunit;

public class EventStoreExtensionsTests
{
    [Fact]
    public void DeserializesSerializedInnerEvent()
    {
        var originalEvent = new MyEvent(42);
        var versionableEvent = new VersionableEvent(originalEvent, 1, DateTimeOffset.Now);
        var headers = new Dictionary<string, object> { { "UserName", "Patrick" }, { "MagicNumber", 42 } };
        var eventData = versionableEvent.SerializeInnerEvent(TypeNameStrategy.Loose, headers);

        var resolvedEvent = CreateResolvedEvent(eventData);
        var deserializedEvent = resolvedEvent.DeserializeAsEvent();

        deserializedEvent.Should().BeEquivalentTo(originalEvent);
    }

    [Fact]
    public void DeserializesSerializedSnapshot()
    {
        var originalSnapshot = new MySnapshot(42, 1, DateTimeOffset.Now);
        var eventData = originalSnapshot.SerializeSnapshot(TypeNameStrategy.Loose);

        var resolvedEvent = CreateResolvedEvent(eventData);
        var deserializedSnapshot = resolvedEvent.DeserializeAsSnapshot();

        deserializedSnapshot.Should().BeEquivalentTo(originalSnapshot);
    }

    private static ResolvedEvent CreateResolvedEvent(EventData eventData)
    {
        var eventRecord = new EventRecord(
            "eventStreamId",
            eventData.EventId,
            new StreamPosition(1),
            new Position(1, 1),
            new Dictionary<string, string>
            {
                { "type", eventData.Type },
                { "content-type", eventData.ContentType },
                { "created", DateTimeOffset.Now.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture) }
            },
            eventData.Data,
            eventData.Metadata
        );

        return new ResolvedEvent(eventRecord, null, 1);
    }
}