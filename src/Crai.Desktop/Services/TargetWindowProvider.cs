using Crai.Application.Contracts.Services;

namespace Crai.Desktop.Services;

public class TargetWindowProvider : ITargetWindowProvider
{
    /// <summary>
    /// Tham chiếu động tới Window đang hoạt động.
    /// </summary>
    public object? TargetWindow { get; set; }

    public object? GetTargetWindow() => TargetWindow;
}
