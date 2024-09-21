namespace GiftcardSample;

using SimpleDomain;

public sealed class UseCases(IEventSourcedRepository repository, Bus bus)
{
    public Task Create(int cardNumber, DateTime validUntil, decimal balance)
    {
        var giftcard = Giftcard.Create(cardNumber, validUntil, balance);
        return repository.Save(giftcard, bus.Publish);
    }

    public async Task Activate(string cardId)
    {
        var giftcard = await repository.GetById<Giftcard>(cardId);
        giftcard.Activate();
        await repository.Save(giftcard, bus.Publish);
    }

    public async Task Redeem(string cardId, decimal amount)
    {
        var giftcard = await repository.GetById<Giftcard>(cardId);
        giftcard.Redeem(amount);
        await repository.Save(giftcard, bus.Publish);
    }

    public async Task Load(string cardId, decimal amount)
    {
        var giftcard = await repository.GetById<Giftcard>(cardId);
        giftcard.Load(amount);
        await repository.Save(giftcard, bus.Publish);
    }
}