namespace GiftcardSample;

using ReadStore;
using SimpleDomain;

public sealed class UseCases(IEventSourcedRepository repository, IWriteStore writeStore)
{
    public Task Create(int cardNumber, DateTime validUntil, decimal balance)
    {
        var giftcard = Giftcard.Create(cardNumber, validUntil, balance);
        return repository.Save(giftcard, e => this.Publish((GiftcardCreated)e));
    }

    public async Task Activate(string cardId)
    {
        var giftcard = await repository.GetById<Giftcard>(cardId);
        giftcard.Activate();
        await repository.Save(giftcard, e => this.Publish((GiftcardActivated)e));
    }

    public async Task Redeem(string cardId, decimal amount)
    {
        var giftcard = await repository.GetById<Giftcard>(cardId);
        giftcard.Redeem(amount);
        await repository.Save(giftcard, e => this.Publish((GiftcardRedeemed)e));
    }

    public async Task Load(string cardId, decimal amount)
    {
        var giftcard = await repository.GetById<Giftcard>(cardId);
        giftcard.Load(amount);
        await repository.Save(giftcard, e => this.Publish((GiftcardLoaded)e));
    }

    private Task Publish(GiftcardCreated giftcardCreated) => writeStore.Add(
        giftcardCreated.CardId,
        giftcardCreated.CardNumber,
        giftcardCreated.Balance,
        giftcardCreated.ValidUntil);

    private Task Publish(GiftcardActivated giftcardActivated) => writeStore.Activate(giftcardActivated.CardId);

    private Task Publish(GiftcardRedeemed giftcardRedeemed) =>
        writeStore.Redeem(giftcardRedeemed.CardId, giftcardRedeemed.Amount);

    private Task Publish(GiftcardLoaded giftcardLoaded) =>
        writeStore.Load(giftcardLoaded.CardId, giftcardLoaded.Amount);
}