using System;
using System.Threading.Tasks;

namespace Crai.Application.Contracts.Infrastructure;

public interface IEventBus
{
    /// <summary>
    /// Phát hành một sự kiện đến tất cả các handler đang đăng ký lắng nghe.
    /// </summary>
    Task PublishAsync<TEvent>(TEvent @event) where TEvent : ICraiEvent;

    /// <summary>
    /// Đăng ký một handler xử lý cho một loại sự kiện cụ thể.
    /// </summary>
    void Subscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : ICraiEvent;

    /// <summary>
    /// Hủy đăng ký một handler xử lý.
    /// </summary>
    void Unsubscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : ICraiEvent;
}
