namespace GiftcardSample;

using MassTransit;
using ReadStore;

internal sealed class GiftcardCreatedHandler(IWriteStore writeStore) : IConsumer<GiftcardCreated>
{
    public Task Consume(ConsumeContext<GiftcardCreated> context)
        => writeStore.Add(
            context.Message.CardId,
            context.Message.CardNumber,
            context.Message.Balance,
            context.Message.ValidUntil);
}

internal sealed class GiftcardActivatedHandler(IWriteStore writeStore) : IConsumer<GiftcardActivated>
{
    public Task Consume(ConsumeContext<GiftcardActivated> context)
        => writeStore.Activate(context.Message.CardId);
}

internal sealed class GiftcardRedeemedHandler(IWriteStore writeStore) : IConsumer<GiftcardRedeemed>
{
    public Task Consume(ConsumeContext<GiftcardRedeemed> context)
        => writeStore.Redeem(context.Message.CardId, context.Message.Amount);
}

internal sealed class GiftcardLoadedHandler(IWriteStore writeStore) : IConsumer<GiftcardLoaded>
{
    public Task Consume(ConsumeContext<GiftcardLoaded> context)
        => writeStore.Load(context.Message.CardId, context.Message.Amount);
}