namespace GiftcardSample.ReadStore;

using FluentAssertions;
using Xunit;

public abstract class InMemoryStoreTests
{
    public class AddingNewGiftcard : IAsyncLifetime
    {
        private const int CardNumber = 12345;
        private static readonly string CardId = $"{CardNumber}";
        private static readonly DateTime ValidUntil = DateTime.Today.AddDays(1);

        private readonly InMemoryStore testee = new();

        public Task InitializeAsync() => this.testee.Add(CardId, CardNumber, 100, ValidUntil);

        public Task DisposeAsync() => Task.CompletedTask;

        [Fact]
        public async Task HasExistingGiftcard()
        {
            var exists = await this.testee.Exists(CardNumber);
            exists.Should().BeTrue();
        }

        [Fact]
        public async Task UpdatesSummary()
        {
            var summary = await this.testee.GetSummary();
            summary.Should().BeEquivalentTo(new { Count = 1, TotalAmount = 100, SumOfExecutedPayments = 0 });
        }

        [Fact]
        public async Task MustNotAddGiftcardOverview()
        {
            var overviews = await this.testee.GetActiveCards();
            overviews.Should().BeEmpty();
        }

        [Fact]
        public async Task AppendsTransaction()
        {
            var transactions = await this.testee.GetTransactions(CardId);
            var lastTransaction = transactions.Last();
            lastTransaction
                .Should()
                .BeEquivalentTo(
                    new
                    {
                        CardId,
                        CardNumber,
                        ValutaDate = DateTime.Today,
                        Event = "Created",
                        Balance = 100,
                        Amount = 0,
                        Revision = 0
                    }
                );
        }
    }

    public class ActivatingExistingGiftcard : IAsyncLifetime
    {
        private const int CardNumber = 12345;
        private static readonly string CardId = $"{CardNumber}";
        private static readonly DateTime ValidUntil = DateTime.Today.AddDays(1);

        private readonly InMemoryStore testee = new();

        public async Task InitializeAsync()
        {
            await this.testee.Add(CardId, CardNumber, 100, ValidUntil);
            await this.testee.Activate(CardId);
        }

        public Task DisposeAsync() => Task.CompletedTask;

        [Fact]
        public async Task AddsGiftcardOverview()
        {
            var overviews = await this.testee.GetActiveCards();
            overviews.Should().ContainSingle()
                .Which
                .Should()
                .BeEquivalentTo(
                    new
                    {
                        CardId,
                        CardNumber,
                        Balance = 100,
                        ValidUntil,
                        Status = GiftcardStatus.Active
                    }
                );
        }

        [Fact]
        public async Task AppendsTransaction()
        {
            var transactions = await this.testee.GetTransactions(CardId);
            var lastTransaction = transactions.Last();
            lastTransaction
                .Should()
                .BeEquivalentTo(
                    new
                    {
                        CardId,
                        CardNumber,
                        ValutaDate = DateTime.Today,
                        Event = "Activated",
                        Balance = 100,
                        Amount = 0,
                        Revision = 1
                    }
                );
        }
    }

    public class RedeemingExistingGiftcard : IAsyncLifetime
    {
        private const int CardNumber = 12345;
        private static readonly string CardId = $"{CardNumber}";
        private static readonly DateTime ValidUntil = DateTime.Today.AddDays(1);

        private readonly InMemoryStore testee = new();

        public async Task InitializeAsync()
        {
            await this.testee.Add(CardId, CardNumber, 100, ValidUntil);
            await this.testee.Activate(CardId);
            await this.testee.Redeem(CardId, 25);
        }

        public Task DisposeAsync() => Task.CompletedTask;

        [Fact]
        public async Task UpdatesSummary()
        {
            var summary = await this.testee.GetSummary();
            summary.Should().BeEquivalentTo(new { Count = 1, TotalAmount = 75, SumOfExecutedPayments = 25 });
        }

        [Fact]
        public async Task UpdatesGiftcardOverview()
        {
            var overviews = await this.testee.GetActiveCards();
            overviews.Should().ContainSingle()
                .Which
                .Should()
                .BeEquivalentTo(
                    new
                    {
                        CardId,
                        CardNumber,
                        Balance = 75,
                        ValidUntil,
                        Status = GiftcardStatus.Active
                    }
                );
        }

        [Fact]
        public async Task AppendsTransaction()
        {
            var transactions = await this.testee.GetTransactions(CardId);
            var lastTransaction = transactions.Last();
            lastTransaction
                .Should()
                .BeEquivalentTo(
                    new
                    {
                        CardId,
                        CardNumber,
                        ValutaDate = DateTime.Today,
                        Event = "Redeemed",
                        Balance = 75,
                        Amount = 25,
                        Revision = 2
                    }
                );
        }
    }

    public class LoadingExistingGiftcard : IAsyncLifetime
    {
        private const int CardNumber = 12345;
        private static readonly string CardId = $"{CardNumber}";
        private static readonly DateTime ValidUntil = DateTime.Today.AddDays(1);

        private readonly InMemoryStore testee = new();

        public async Task InitializeAsync()
        {
            await this.testee.Add(CardId, CardNumber, 100, ValidUntil);
            await this.testee.Activate(CardId);
            await this.testee.Load(CardId, 50);
        }

        public Task DisposeAsync() => Task.CompletedTask;

        [Fact]
        public async Task UpdatesSummary()
        {
            var summary = await this.testee.GetSummary();
            summary.Should().BeEquivalentTo(new { Count = 1, TotalAmount = 150, SumOfExecutedPayments = 0 });
        }

        [Fact]
        public async Task UpdatesGiftcardOverview()
        {
            var overviews = await this.testee.GetActiveCards();
            overviews.Should().ContainSingle()
                .Which
                .Should()
                .BeEquivalentTo(
                    new
                    {
                        CardId,
                        CardNumber,
                        Balance = 150,
                        ValidUntil,
                        Status = GiftcardStatus.Active
                    }
                );
        }

        [Fact]
        public async Task AppendsTransaction()
        {
            var transactions = await this.testee.GetTransactions(CardId);
            var lastTransaction = transactions.Last();
            lastTransaction
                .Should()
                .BeEquivalentTo(
                    new
                    {
                        CardId,
                        CardNumber,
                        ValutaDate = DateTime.Today,
                        Event = "Loaded",
                        Balance = 150,
                        Amount = 50,
                        Revision = 2
                    }
                );
        }
    }
}