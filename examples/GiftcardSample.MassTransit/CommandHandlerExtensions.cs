namespace GiftcardSample;

using MassTransit;

public static class CommandHandlerExtensions
{
    public static void AddCommandHandlers(this IBusRegistrationConfigurator configurator)
    {
        configurator.AddConsumer<CreateGiftcardHandler>();
        configurator.AddConsumer<ActivateGiftcardHandler>();
        configurator.AddConsumer<RedeemGiftcardHandler>();
        configurator.AddConsumer<LoadGiftcardHandler>();
    }
}