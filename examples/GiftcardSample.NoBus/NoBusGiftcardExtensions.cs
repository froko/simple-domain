namespace GiftcardSample;

using Microsoft.Extensions.DependencyInjection;

public static class NoBusGiftcardExtensions
{
    public static IServiceCollection AddGiftcardUseCases(this IServiceCollection services)
    {
        services.AddTransient<UseCases>();
        return services;
    }
}