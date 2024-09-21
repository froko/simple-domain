namespace GiftcardSample;

using Microsoft.Extensions.DependencyInjection;

public static class ReactiveGiftcardExtensions
{
    public static IServiceCollection AddReactiveGiftcardUseCases(this IServiceCollection services)
    {
        services.AddSingleton<Bus>();
        services.AddSingleton<EventHandlers>();
        services.AddTransient<UseCases>();

        return services;
    }
}