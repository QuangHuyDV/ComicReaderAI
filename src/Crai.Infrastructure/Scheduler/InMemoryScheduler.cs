using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Crai.Application.Contracts.Infrastructure;

namespace Crai.Infrastructure.Scheduler;

public class InMemoryScheduler : IScheduler
{
    private readonly ConcurrentDictionary<string, ScheduledTaskInfo> _tasks = new();
    private readonly IStructuredLogger _logger;
    private bool _isShutdown;

    public InMemoryScheduler(IStructuredLogger logger)
    {
        _logger = logger;
    }

    public void RegisterTask(TaskDefinition definition)
    {
        if (_isShutdown)
        {
            _logger.LogWarning($"[Scheduler] Không thể đăng ký task '{definition.Name}' do scheduler đã shutdown.");
            return;
        }

        // Hủy task cũ trùng Id nếu tồn tại
        if (_tasks.ContainsKey(definition.Id))
        {
            CancelTask(definition.Id);
        }

        var cts = new CancellationTokenSource();
        var runningTask = Task.Run(async () =>
        {
            try
            {
                var token = cts.Token;

                // 1. Thực hiện Initial Delay nếu có
                if (definition.InitialDelay.HasValue && definition.InitialDelay.Value > TimeSpan.Zero)
                {
                    await Task.Delay(definition.InitialDelay.Value, token);
                }

                if (definition.Interval.HasValue)
                {
                    // 2. Loop chạy định kỳ (Periodic task)
                    while (!token.IsCancellationRequested)
                    {
                        await RunTaskActionSafeAsync(definition, token);
                        await Task.Delay(definition.Interval.Value, token);
                    }
                }
                else
                {
                    // 3. Chạy một lần duy nhất (One-shot task)
                    await RunTaskActionSafeAsync(definition, token);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogDebug($"[Scheduler] Task '{definition.Name}' (Id: {definition.Id}) đã bị hủy bỏ thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"[Scheduler] Lỗi nghiêm trọng ở background loop của task '{definition.Name}': {ex.Message}", ex);
            }
            finally
            {
                // Tự động dọn dẹp nếu là one-shot task
                if (!definition.Interval.HasValue)
                {
                    _tasks.TryRemove(definition.Id, out _);
                }
            }
        });

        var info = new ScheduledTaskInfo(definition, cts, runningTask);
        _tasks[definition.Id] = info;

        _logger.LogDebug($"[Scheduler] Đã đăng ký thành công task '{definition.Name}' (Id: {definition.Id}, Interval: {definition.Interval?.TotalSeconds}s)");
    }

    public void TriggerNow(string taskId)
    {
        if (_tasks.TryGetValue(taskId, out var info))
        {
            _logger.LogDebug($"[Scheduler] Kích hoạt chạy thủ công task '{info.Definition.Name}' ngay lập tức.");
            // Thực thi action bất đồng bộ trên thread riêng biệt mà không ảnh hưởng tới loop hiện tại
            Task.Run(async () =>
            {
                try
                {
                    await info.Definition.Action(CancellationToken.None);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"[Scheduler] Lỗi khi chạy thủ công task '{info.Definition.Name}': {ex.Message}", ex);
                }
            });
        }
        else
        {
            _logger.LogWarning($"[Scheduler] Không tìm thấy task với Id '{taskId}' để chạy.");
        }
    }

    public void CancelTask(string taskId)
    {
        if (_tasks.TryRemove(taskId, out var info))
        {
            _logger.LogDebug($"[Scheduler] Đang yêu cầu dừng task '{info.Definition.Name}' (Id: {taskId})...");
            info.Cts.Cancel();
        }
    }

    public void Shutdown()
    {
        if (_isShutdown) return;
        _isShutdown = true;

        _logger.LogInfo("[Scheduler] Đang tắt hệ thống background scheduler...");

        var tasksToWait = new List<Task>();
        foreach (var key in _tasks.Keys.ToList())
        {
            if (_tasks.TryRemove(key, out var info))
            {
                info.Cts.Cancel();
                tasksToWait.Add(info.RunningTask);
            }
        }

        try
        {
            // Đợi tối đa 2 giây cho tất cả các task hoàn thành
            Task.WaitAll(tasksToWait.ToArray(), 2000);
            _logger.LogInfo("[Scheduler] Đã tắt sạch toàn bộ background tasks an toàn.");
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"[Scheduler] Có lỗi hoặc timeout khi tắt các background tasks: {ex.Message}");
        }
    }

    private async Task RunTaskActionSafeAsync(TaskDefinition definition, CancellationToken token)
    {
        try
        {
            _logger.LogDebug($"[Scheduler] Đang thực thi task '{definition.Name}'");
            await definition.Action(token);
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            // Bỏ qua nếu bị cancel đúng luồng
        }
        catch (Exception ex)
        {
            _logger.LogError($"[Scheduler] Task '{definition.Name}' quăng lỗi khi thực thi: {ex.Message}", ex);
        }
    }

    private class ScheduledTaskInfo
    {
        public TaskDefinition Definition { get; }
        public CancellationTokenSource Cts { get; }
        public Task RunningTask { get; }

        public ScheduledTaskInfo(TaskDefinition definition, CancellationTokenSource cts, Task runningTask)
        {
            Definition = definition;
            Cts = cts;
            RunningTask = runningTask;
        }
    }
}
