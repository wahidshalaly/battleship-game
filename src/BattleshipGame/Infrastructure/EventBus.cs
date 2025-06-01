using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using BattleshipGame.Domain.Events;

namespace BattleshipGame.Infrastructure;

public interface IEventBus : IAsyncDisposable
{
    ValueTask PublishAsync(IDomainEvent evt, CancellationToken ct);
    ValueTask<IDomainEvent> ReadAsync<T>(CancellationToken ct)
        where T : IDomainEvent;
}

internal class EventBus : IEventBus
{
    private readonly Channel<IDomainEvent> _eventsChannel = Channel.CreateBounded<IDomainEvent>(
        new BoundedChannelOptions(1000)
    );

    public ValueTask PublishAsync(IDomainEvent evt, CancellationToken ct)
    {
        return _eventsChannel.Writer.WriteAsync(evt, ct);
    }

    public ValueTask<IDomainEvent> ReadAsync<T>(CancellationToken ct)
        where T : IDomainEvent
    {
        return _eventsChannel.Reader.ReadAsync(ct);
    }

    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }
}
