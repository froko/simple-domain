namespace GiftcardSample.ReadStore;

public interface IReadStore
{
    Task<bool> Exists(int cardNumber);

    Task<GiftcardSummary> GetSummary();

    Task<IReadOnlyCollection<GiftcardOverview>> GetActiveCards();

    Task<GiftcardOverview?> GetOverview(string cardId);

    Task<IReadOnlyCollection<GiftcardTransaction>> GetTransactions(string cardId);
}