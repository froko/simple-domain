namespace SimpleDomain;

using EventStore;

/// <summary>
/// The EventStore aware SimpleDomain configuration interface.
/// </summary>
public interface IConfigureSimpleDomainWithEventStore : IConfigureSimpleDomain
{
    /// <summary>
    /// Use the EventStore service.
    /// </summary>
    /// <param name="connectionString">The connection string to the EventStore service.</param>
    /// <param name="typeNameStrategy">The type name serialization strategy.</param>
    /// <returns>An instance of <see cref="IConfigureEventStore" /> since this is a fluent configuration.</returns>
    IConfigureEventStore UseEventStore(string connectionString, TypeNameStrategy? typeNameStrategy = null);
}