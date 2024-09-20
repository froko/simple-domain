namespace SimpleDomain;

/// <summary>
/// Provides a read-only timestamp property for events and snapshots.
/// </summary>
public interface INeedTimestamp
{
    /// <summary>
    /// Gets the timestamp of the event or snapshot.
    /// </summary>
    DateTimeOffset Timestamp { get; }
}