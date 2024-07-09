---
title: Getting Started
---

This Getting Started guide is built around a Giftcard sample application as it is much easier to explain the steps and
concepts using a (near) real-world example.

What you'll find here is a straight-forward step-by-step guide to get you started with SimpleDomain.
It is not a comprehensive guide. For more detailed information, please refer to the [Documentation](/simple-domain/documentation).

## Prerequisites

Before you start, you need to have the following prerequisites:

- .NET 8 SDK or later (download from [here](https://dotnet.microsoft.com/download))
- Visual Studio Code (download from [here](https://code.visualstudio.com/)) or another IDE of your choice

In the following sections we will use the .NET CLI to create projects, add packages, and run the application.
As you may are familiar with your IDE, you can use it to perform the same tasks.

## Create a new project

Create a new library project using the .NET CLI:

```bash
dotnet new classlib -n Giftcard
```

## Add the SimpleDomain package

Add the SimpleDomain package to the project:

```bash
dotnet add package SimpleDomain
```

## Define the events

Imagine you had an EventStorming session with your stakeholders and you came up with the following events:

- GiftcardCreated
- GiftcardActivated
- GiftcardRedeemed
- GiftcardLoaded

Create a new file named `Events.cs` in the project and add the following code:

```csharp
namespace GiftcardSample;

using SimpleDomain;

public record GiftcardCreated(int CardNumber, decimal Balance, DateTime ValidUntil) : IEvent
{
    public string CardId => $"{this.CardNumber}";
}

public record GiftcardActivated(string CardId) : IEvent;

public record GiftcardRedeemed(string CardId, decimal Amount) : IEvent;

public record GiftcardLoaded(string CardId, decimal Amount) : IEvent;
```

As you can see, we are using the `IEvent` interface from the `SimpleDomain` package.
The interface acts a a marker to let the framework know that this is an event. The `IEvent` interface does not have any
members so you don't have to implement anything.

The only thing you have to make sure is that the events have a unique string identifier. In this case, we use the
`CardId` property for that. The string identifier is used by the framework to correlate the aggregates with their events.

## Add the Giftcard aggregate

### First skeleton

Create a new file named `Giftcard.cs` in the project and add the following code:

```csharp
using SimpleDomain;

namespace GiftcardSample;

public sealed class Giftcard : StaticEventSourcedAggregateRoot
{
    private decimal balance;
    private DateTime validUntil;
    private bool isActive;

    // The default constructor is required by the framework
    public Giftcard()
    {
        // Todo: Register tranistions
    }

    private Giftcard(int cardNumber, DateTime validUntil, decimal initialBalance) : this()
    {
        // Todo: Apply GiftcardCreated event
    }

    public static Giftcard Create(int cardNumber, DateTime validUntil, decimal initialBalance = 0)
    {
        // Todo: Add business rules
        return new Giftcard(cardNumber, validUntil, initialBalance);
    }
}

```

The `Giftcard` class inherits from `StaticEventSourcedAggregateRoot` which is a base class provided by `SimpleDomain`.
The base class provides the necessary infrastructure to store uncommitted events and apply events from the store to the aggregate.

Internal state is stored in private fields. In a CQRS architecture, the aggregates should not expose their state to the
outside world. Instead we are using the events to populate optimized read models that can be queried.

Aggregate roots in `SimpleDomain` are required to have a default constructor. This constructor is used by the framework
and should register the transitions between the events and the methods that apply them. We will add them later.

The `Giftcard` aggregate offers a static factory method to create a new instance with all the required information using
a private constructor. The factory method is a good place to add business rules.

### Add the first transition

When a `Giftcard` is created, a `GiftcardCreated` event should be applied to the aggregate. This event is used to
initialize the aggregate state.

Modify the `Giftcard` class as follows:

```csharp
using SimpleDomain;

namespace GiftcardSample;

public sealed class Giftcard : StaticEventSourcedAggregateRoot
{
    private decimal balance;
    private DateTime validUntil;
    private bool isActive;

    public Giftcard()
    {
        this.RegisterTransition<GiftcardCreated>(this.Apply);
    }

    private Giftcard(int cardNumber, DateTime validUntil, decimal initialBalance) : this()
        => this.ApplyEvent(new GiftcardCreated(cardNumber, initialBalance, validUntil));

    public static Giftcard Create(int cardNumber, DateTime validUntil, decimal initialBalance = 0)
    {
        // Todo: Add business rules
        return new Giftcard(cardNumber, validUntil, initialBalance);
    }

    private void Apply(GiftcardCreated @event)
    {
        this.Id = $"{@event.CardNumber}";
        this.balance = @event.Balance;
        this.validUntil = @event.ValidUntil;
        this.isActive = false;
    }
}

```

The `RegisterTransition` method is used to register the transition between the `GiftcardCreated` event and the `Apply`
method. The `Apply` method is called by the protected `ApplyEvent` method to apply the event to the aggregate.
It's important to note that all the state changes should be done in the `Apply` methods because they are also called
when the aggregate is rehydrated from the event store. `Apply` mehthods must not fail, otherwise the aggregate cannot be
rehydrated to its desired state.

### Add business rules

To make sure that the `Giftcard` is created with a valid state, we need to add some business rules. Let's add them for
our first `Create` method:

```csharp
public static Giftcard Create(int cardNumber, DateTime validUntil, decimal initialBalance = 0)
{
    Guard.Argument(() => cardNumber >= 0, "Negative card number");
    Guard.Argument(() => validUntil >= DateTime.Today, "Already expired");
    Guard.Argument(() => initialBalance >= 0, "Negative initial balance");

    return new Giftcard(cardNumber, validUntil, initialBalance);
}
```

The static `Guard` class is a helper class provided by `SimpleDomain` to add preconditions to the code. If a precondition
fails, an Exception is thrown, which needs to be caught by the caller. We will use other methods of the `Guard` class
later in this guide. A documentation can be found [here](/simple-domain/documentation/guard).

### Add remaining transitions and business rules

Add the following transitions and business rules to the `Giftcard` class:

```csharp
public sealed class Giftcard : StaticEventSourcedAggregateRoot
{
    private decimal balance;
    private DateTime validUntil;
    private bool isActive;

    public Giftcard()
    {
        ...
        this.RegisterTransition<GiftcardActivated>(this.Apply);
        this.RegisterTransition<GiftcardLoaded>(this.Apply);
        this.RegisterTransition<GiftcardRedeemed>(this.Apply);
    }

    ...

    public void Activate()
    {
        Guard.State(() => this.validUntil >= DateTime.Today, "Already expired");
        Guard.State(() => !this.isActive, "Already active");

        this.ApplyEvent(new GiftcardActivated(this.Id));
    }

    public void Load(decimal amount)
    {
        Guard.Argument(() => amount > 0, "Negative amount");
        Guard.State(() => this.isActive, "Not active");
        Guard.State(() => this.validUntil >= DateTime.Today, "Already expired");

        this.ApplyEvent(new GiftcardLoaded(this.Id, amount));
    }

    public void Redeem(decimal amount)
    {
        Guard.Argument(() => amount > 0, "Negative amount");
        Guard.State(() => this.isActive, "Not active");
        Guard.State(() => this.validUntil >= DateTime.Today, "Already expired");
        Guard.State(() => amount <= this.balance, "Insufficient balance");

        this.ApplyEvent(new GiftcardRedeemed(this.Id, amount));
    }

    ...

    private void Apply(GiftcardActivated @event) => this.isActive = true;

    private void Apply(GiftcardLoaded @event) => this.balance += @event.Amount;

    private void Apply(GiftcardRedeemed @event) => this.balance -= @event.Amount;
}

```
