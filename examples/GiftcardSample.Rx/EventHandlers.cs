namespace GiftcardSample;

using ReadStore;

public sealed class EventHandlers(Bus bus, IWriteStore writeStore) : IDisposable
{
    private IDisposable? subscription;

    public void Dispose() => this.subscription?.Dispose();

    public void Subscribe() =>
        this.subscription = bus.EventStream.Subscribe(@event =>
        {
            switch (@event)
            {
                case GiftcardCreated giftcardCreated:
                    writeStore.Add(
                        giftcardCreated.CardId,
                        giftcardCreated.CardNumber,
                        giftcardCreated.Balance,
                        giftcardCreated.ValidUntil);
                    break;
                case GiftcardActivated giftcardActivated:
                    writeStore.Activate(giftcardActivated.CardId);
                    break;
                case GiftcardRedeemed giftcardRedeemed:
                    writeStore.Redeem(giftcardRedeemed.CardId, giftcardRedeemed.Amount);
                    break;
                case GiftcardLoaded giftcardLoaded:
                    writeStore.Load(giftcardLoaded.CardId, giftcardLoaded.Amount);
                    break;
            }
        });
}