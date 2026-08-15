using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Crai.Application.Contracts.Infrastructure;

namespace Crai.Infrastructure.EventBus;

public class InMemoryEventBus : IEventBus
{
    private readonly ConcurrentDictionary<Type, List<object>> _handlers = new();
    private readonly IStructuredLogger _logger;

    public InMemoryEventBus(IStructuredLogger logger)
    {
        _logger = logger;
    }

    public void Subscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : ICraiEvent
    {
        var eventType = typeof(TEvent);
        _handlers.AddOrUpdate(eventType,
            _ => new List<object> { handler },
            (_, list) =>
            {
                lock (list)
                {
                    if (!list.Contains(handler))
                    {
                        list.Add(handler);
                    }
                }
                return list;
            });

        _logger.LogDebug($"[EventBus] Đã subscribe handler '{handler.GetType().Name}' cho event '{eventType.Name}'");
    }

    public void Unsubscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : ICraiEvent
    {
        var eventType = typeof(TEvent);
        if (_handlers.TryGetValue(eventType, out var list))
        {
            lock (list)
            {
                list.Remove(handler);
            }
            _logger.LogDebug($"[EventBus] Đã unsubscribe handler '{handler.GetType().Name}' từ event '{eventType.Name}'");
        }
    }

    public async Task PublishAsync<TEvent>(TEvent @event) where TEvent : ICraiEvent
    {
        var eventType = @event.GetType();
        _logger.LogDebug($"[EventBus] Đang phát hành event '{eventType.Name}' (Id: {@event.Id})");

        if (!_handlers.TryGetValue(eventType, out var list))
        {
            return;
        }

        List<object> handlersCopy;
        lock (list)
        {
            handlersCopy = list.ToList();
        }

        var tasks = handlersCopy.Select(async handler =>
        {
            try
            {
                if (handler is IEventHandler<TEvent> typedHandler)
                {
                    await typedHandler.HandleAsync(@event);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"[EventBus] Lỗi khi xử lý event '{eventType.Name}' ở handler '{handler.GetType().Name}': {ex.Message}", ex);
            }
        });

        await Task.WhenAll(tasks);
    }
}
