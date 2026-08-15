using System.Threading.Tasks;

namespace Crai.Application.Contracts.Infrastructure;

public interface IEventHandler<in TEvent> where TEvent : ICraiEvent
{
    /// <summary>
    /// Hàm xử lý sự kiện bất đồng bộ.
    /// </summary>
    Task HandleAsync(TEvent @event);
}
