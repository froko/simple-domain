namespace GiftcardSample.ReadStore;

public interface IWriteStore
{
    Task Add(string cardId, int cardNumber, decimal balance, DateTime validUntil);

    Task Activate(string cardId);

    Task Redeem(string cardId, decimal amount);

    Task Load(string cardId, decimal amount);
}