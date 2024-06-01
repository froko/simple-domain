namespace GiftcardSample.ReadStore;

using Microsoft.Extensions.DependencyInjection;

public static class ReadStoreExtensions
{
    public static IServiceCollection AddReadStore(this IServiceCollection services)
    {
        var inMemoryStore = new InMemoryStore();
        services.AddSingleton<IReadStore>(inMemoryStore);
        services.AddSingleton<IWriteStore>(inMemoryStore);
        return services;
    }
}