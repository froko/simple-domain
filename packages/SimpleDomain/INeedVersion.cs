namespace SimpleDomain;

/// <summary>
/// Provides a read-only version property for events and snapshots.
/// </summary>
public interface INeedVersion
{
    /// <summary>
    /// Gets the current version.
    /// </summary>
    int Version { get; }
}