namespace GiftcardSample;

using MassTransit;
using SimpleDomain;

internal sealed class CreateGiftcardHandler(IEventSourcedRepository repository) : IConsumer<CreateGiftcard>
{
    public Task Consume(ConsumeContext<CreateGiftcard> context)
    {
        var message = context.Message;
        var giftcard = Giftcard.Create(
            message.CardNumber,
            message.ValidUntil,
            message.Balance);
        return repository.Save(giftcard, e => context.Publish(e));
    }
}

internal sealed class ActivateGiftcardHandler(IEventSourcedRepository repository) : IConsumer<ActivateGiftcard>
{
    public async Task Consume(ConsumeContext<ActivateGiftcard> context)
    {
        var message = context.Message;
        var giftcard = await repository.GetById<Giftcard>(message.CardId);
        giftcard.Activate();
        await repository.Save(giftcard, e => context.Publish(e));
    }
}

internal sealed class RedeemGiftcardHandler(IEventSourcedRepository repository) : IConsumer<RedeemGiftcard>
{
    public async Task Consume(ConsumeContext<RedeemGiftcard> context)
    {
        var message = context.Message;
        var giftcard = await repository.GetById<Giftcard>(message.CardId);
        giftcard.Redeem(message.Amount);
        await repository.Save(giftcard, e => context.Publish(e));
    }
}

internal sealed class LoadGiftcardHandler(IEventSourcedRepository repository) : IConsumer<LoadGiftcard>
{
    public async Task Consume(ConsumeContext<LoadGiftcard> context)
    {
        var message = context.Message;
        var giftcard = await repository.GetById<Giftcard>(message.CardId);
        giftcard.Load(message.Amount);
        await repository.Save(giftcard, e => context.Publish(e));
    }
}