namespace SimpleDomain;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Some extensions for <see cref="IServiceCollection" />.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds SimpleDomain assets to the dependency injection container.
    /// </summary>
    /// <param name="serviceCollection">The service collection.</param>
    /// <param name="configure">The action to configure the dependencies.</param>
    /// <returns>The service collection itself since this is a builder method.</returns>
    public static IServiceCollection AddSimpleDomain(
        this IServiceCollection serviceCollection,
        Action<IConfigureSimpleDomain> configure)
    {
        var configurator = new SimpleDomainConfigurator(serviceCollection);
        configure.Invoke(configurator);
        configurator.Complete();
        return serviceCollection;
    }
}