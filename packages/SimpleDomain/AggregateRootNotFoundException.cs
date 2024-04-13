namespace SimpleDomain;

/// <summary>
/// The exception that is thrown when an aggregate root could not be found by its key.
/// </summary>
public class AggregateRootNotFoundException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateRootNotFoundException"/> class.
    /// </summary>
    /// <param name="aggregateType">The type of the aggregate root.</param>
    /// <param name="aggregateKey">The key of the aggregate root.</param>
    public AggregateRootNotFoundException(Type aggregateType, string aggregateKey)
        : base($"An aggregate of type {aggregateType} with key {aggregateKey} could not be found.") { }
}
