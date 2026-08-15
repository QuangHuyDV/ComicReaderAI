using System;
using System.Threading;
using System.Threading.Tasks;

namespace Crai.Application.Contracts.Infrastructure;

public class TaskDefinition
{
    /// <summary>
    /// Mã định danh duy nhất của task.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Tên mô tả thân thiện của task.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Action thực thi logic nghiệp vụ của task.
    /// </summary>
    public Func<CancellationToken, Task> Action { get; }

    /// <summary>
    /// Chu kỳ lặp lại (nếu có). Nếu null nghĩa là chỉ chạy một lần duy nhất (one-shot).
    /// </summary>
    public TimeSpan? Interval { get; }

    /// <summary>
    /// Thời gian chờ trước khi chạy lần đầu tiên.
    /// </summary>
    public TimeSpan? InitialDelay { get; }

    public TaskDefinition(
        string id, 
        string name, 
        Func<CancellationToken, Task> action, 
        TimeSpan? interval = null, 
        TimeSpan? initialDelay = null)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Action = action ?? throw new ArgumentNullException(nameof(action));
        Interval = interval;
        InitialDelay = initialDelay;
    }
}
