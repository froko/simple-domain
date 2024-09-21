namespace GiftcardSample;

using System.Reactive.Subjects;

public sealed class Bus : IDisposable
{
    private readonly Subject<object> eventStream = new();

    public IObservable<object> EventStream => this.eventStream;

    public void Dispose() => this.eventStream.Dispose();

    public Task Publish<TMessage>(TMessage message)
    {
        if (message is object @event) this.eventStream.OnNext(@event);
        return Task.CompletedTask;
    }
}