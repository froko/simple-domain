namespace SimpleDomain;

using FluentAssertions;

/// <summary>
/// The event source aggregate root harness.
/// </summary>
/// <typeparam name="TAggregateRoot">The type of the aggregate root.</typeparam>
public class EventSourcedAggregateRootHarness<TAggregateRoot>
    where TAggregateRoot : EventSourcedAggregateRoot
{
    private Exception aggregateException = null!;

    private TAggregateRoot aggregateRoot;
    private Func<TAggregateRoot> create = Activator.CreateInstance<TAggregateRoot>;
    private IList<object> eventHistory = [];
    private Action<TAggregateRoot> execute = _ => { };

    /// <summary>
    /// Initializes a new instance of the <see cref="EventSourcedAggregateRootHarness{TAggregateRoot}" /> class.
    /// </summary>
    public EventSourcedAggregateRootHarness() => this.aggregateRoot = this.create();

    /// <summary>
    /// Sets the aggregate root under test into the desired state
    /// by replaying events from history.
    /// </summary>
    /// <param name="events">The event history.</param>
    public void LoadFromHistory(params object[] events) => this.eventHistory = new List<object>(events);

    /// <summary>
    /// Creates a new aggregate root under test.
    /// You only need to do this if you want to test a no parameterless constructor.
    /// </summary>
    /// <param name="func">The function to create the aggregate root.</param>
    public void Create(Func<TAggregateRoot> func) => this.create = func;

    /// <summary>
    /// Executes behavior of the aggregate root under test.
    /// </summary>
    /// <param name="action">The action to perform against the aggregate root.</param>
    public void Execute(Action<TAggregateRoot> action) => this.execute = action;

    /// <summary>
    /// Checks the occurrence of a specific exception.
    /// </summary>
    /// <typeparam name="TException">The type of the exception.</typeparam>
    /// <param name="message">The expected exception message.</param>
    public void ShouldFailWith<TException>(string message)
        where TException : Exception
    {
        this.Run();
        this.aggregateException.Should().NotBeNull();
        this.aggregateException.Should().BeOfType<TException>();
        this.aggregateException.Message.Should().Be(message);
    }

    /// <summary>
    /// Checks the occurrence of a specific event.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    public void ShouldEmitEventLike<TEvent>(Func<TEvent, bool> expectation)
        where TEvent : class
    {
        this.Run();
        this.aggregateRoot.UncommittedEvents.OfType<VersionableEvent>()
            .Select(e => e.InnerEvent as TEvent)
            .Should()
            .ContainSingle(e => expectation(e!));
    }

    /// <summary>
    /// Checks the occurrence of a specific event.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    /// <param name="expectedEvent">An instance of the expected event in order to compare the properties.</param>
    public void ShouldEmitEventLike<TEvent>(TEvent expectedEvent)
        where TEvent : class
    {
        this.Run();
        this.CheckForUncommittedEventContent(expectedEvent);
    }

    private void Run()
    {
        try
        {
            this.aggregateRoot = this.create();
            this.aggregateRoot.LoadFromEventHistory(this.eventHistory.ToAsyncEnumerable()).Wait();

            this.execute(this.aggregateRoot);
        }
        catch (Exception exception)
        {
            this.aggregateException = exception;
        }
    }

    private void CheckForUncommittedEventContent<TEvent>(TEvent expectedEvent)
        where TEvent : class
    {
        var actualEvent =
            this.aggregateRoot.UncommittedEvents.OfType<VersionableEvent>().Select(e => e.InnerEvent).Last() as TEvent;

        actualEvent
            ?.Should()
            .BeEquivalentTo(
                expectedEvent,
                options =>
                    options
                        .Using<DateTime>(
                            ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, TimeSpan.FromSeconds(1)))
                        .WhenTypeIs<DateTime>()
            );
    }
}