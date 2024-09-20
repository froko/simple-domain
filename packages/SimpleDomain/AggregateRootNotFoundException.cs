namespace SimpleDomain;

/// <summary>
/// The exception that is thrown when an aggregate root could not be found by its key.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="AggregateRootNotFoundException" /> class.
/// </remarks>
/// <param name="aggregateType">The type of the aggregate root.</param>
/// <param name="aggregateKey">The key of the aggregate root.</param>
public class AggregateRootNotFoundException(Type aggregateType, string aggregateKey)
    : Exception($"An aggregate of type {aggregateType} with key {aggregateKey} could not be found.");