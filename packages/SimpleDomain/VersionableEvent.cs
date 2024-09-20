namespace SimpleDomain;

/// <summary>
/// Wrapper around an event which carries a version property.
/// </summary>
/// <param name="InnerEvent">The original event.</param>
/// <param name="Version">The version of the event.</param>
/// <param name="Timestamp">The timestamp of the event.</param>
public record VersionableEvent(object InnerEvent, int Version, DateTimeOffset Timestamp)
    : INeedVersion, INeedTimestamp;