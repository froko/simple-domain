namespace GiftcardSample;

using MassTransit;

public static class EventHandlerExtensions
{
    public static void AddEventHandlers(this IBusRegistrationConfigurator configurator)
    {
        configurator.AddConsumer<GiftcardCreatedHandler>();
        configurator.AddConsumer<GiftcardActivatedHandler>();
        configurator.AddConsumer<GiftcardRedeemedHandler>();
        configurator.AddConsumer<GiftcardLoadedHandler>();
    }
}