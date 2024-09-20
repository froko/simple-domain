namespace GiftcardSample.ReadStore;

public sealed class InMemoryStore : IReadStore, IWriteStore
{
    private readonly List<int> cardNumbers = [];
    private readonly List<GiftcardOverview> giftcardOverviews = [];
    private readonly List<GiftcardTransaction> giftcardTransactions = [];
    private GiftcardSummary summary = new(0, 0, 0);

    public Task<bool> Exists(int cardNumber)
        => Task.FromResult(this.cardNumbers.Contains(cardNumber));

    public Task<GiftcardSummary> GetSummary()
        => Task.FromResult(this.summary);

    public Task<IReadOnlyCollection<GiftcardOverview>> GetActiveCards()
        => Task.FromResult<IReadOnlyCollection<GiftcardOverview>>(
            this.giftcardOverviews.Where(g => g.Status == GiftcardStatus.Active).ToList());

    public Task<GiftcardOverview?> GetOverview(string cardId)
        => Task.FromResult(this.giftcardOverviews.FirstOrDefault(g => g.CardId == cardId));

    public Task<IReadOnlyCollection<GiftcardTransaction>> GetTransactions(string cardId)
        => Task.FromResult<IReadOnlyCollection<GiftcardTransaction>>(
            this.giftcardTransactions.Where(t => t.CardId == cardId).ToList());

    public Task Add(string cardId, int cardNumber, decimal balance, DateTime validUntil)
    {
        this.summary = this.summary with
        {
            Count = this.summary.Count + 1,
            TotalAmount = this.summary.TotalAmount + balance
        };
        this.cardNumbers.Add(cardNumber);
        this.giftcardOverviews.Add(
            new GiftcardOverview(cardId, cardNumber, balance, validUntil, GiftcardStatus.NotActive));
        this.giftcardTransactions.Add(
            new GiftcardTransaction(cardId, cardNumber, DateTime.Today, "Created", balance, 0, 0));

        return Task.CompletedTask;
    }

    public Task Activate(string cardId)
    {
        var overview = this.giftcardOverviews.First(g => g.CardId == cardId);
        this.giftcardOverviews[this.giftcardOverviews.IndexOf(overview)] =
            overview with { Status = GiftcardStatus.Active };

        var lastTransaction = this.giftcardTransactions.OrderBy(t => t.ValutaDate).Last(t => t.CardId == cardId);
        this.giftcardTransactions.Add(
            new GiftcardTransaction(
                overview.CardId,
                overview.CardNumber,
                DateTime.Today,
                "Activated",
                overview.Balance,
                0,
                lastTransaction.Revision + 1));

        return Task.CompletedTask;
    }

    public Task Redeem(string cardId, decimal amount)
    {
        this.summary = this.summary with
        {
            TotalAmount = this.summary.TotalAmount - amount,
            SumOfExecutedPayments = this.summary.SumOfExecutedPayments + amount
        };

        var overview = this.giftcardOverviews.First(g => g.CardId == cardId);
        this.giftcardOverviews[this.giftcardOverviews.IndexOf(overview)] =
            overview with { Balance = overview.Balance - amount };

        var lastTransaction = this.giftcardTransactions.OrderBy(t => t.ValutaDate).Last(t => t.CardId == cardId);
        this.giftcardTransactions.Add(
            new GiftcardTransaction(
                overview.CardId,
                overview.CardNumber,
                DateTime.Today,
                "Redeemed",
                overview.Balance - amount,
                amount,
                lastTransaction.Revision + 1));

        return Task.CompletedTask;
    }

    public Task Load(string cardId, decimal amount)
    {
        this.summary = this.summary with
        {
            TotalAmount = this.summary.TotalAmount + amount
        };

        var overview = this.giftcardOverviews.First(g => g.CardId == cardId);
        this.giftcardOverviews[this.giftcardOverviews.IndexOf(overview)] =
            overview with { Balance = overview.Balance + amount };

        var lastTransaction = this.giftcardTransactions.OrderBy(t => t.ValutaDate).Last(t => t.CardId == cardId);
        this.giftcardTransactions.Add(
            new GiftcardTransaction(
                overview.CardId,
                overview.CardNumber,
                DateTime.Today,
                "Loaded",
                overview.Balance + amount,
                amount,
                lastTransaction.Revision + 1));

        return Task.CompletedTask;
    }
}