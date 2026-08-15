using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Crai.Application.Contracts.Infrastructure;
using Crai.Infrastructure.Scheduler;

namespace Crai.Infrastructure.Tests;

public class SchedulerTests
{
    private readonly InMemoryScheduler _scheduler;
    private readonly MockLogger _mockLogger;

    public SchedulerTests()
    {
        _mockLogger = new MockLogger();
        _scheduler = new InMemoryScheduler(_mockLogger);
    }

    [Fact]
    public async Task RegisterTask_ShouldExecuteOneShotTask()
    {
        // Arrange
        var executed = false;
        var tcs = new TaskCompletionSource<bool>();

        var taskDef = new TaskDefinition(
            "oneshot_test",
            "One-shot Test Task",
            token =>
            {
                executed = true;
                tcs.SetResult(true);
                return Task.CompletedTask;
            },
            interval: null,
            initialDelay: TimeSpan.FromMilliseconds(10)
        );

        // Act
        _scheduler.RegisterTask(taskDef);

        // Chờ tối đa 500ms cho task hoàn thành
        var completed = await Task.WhenAny(tcs.Task, Task.Delay(500)) == tcs.Task;

        // Assert
        Assert.True(completed);
        Assert.True(executed);
    }

    [Fact]
    public async Task RegisterTask_ShouldRunPeriodicTaskMultipleTimes()
    {
        // Arrange
        var count = 0;
        var taskDef = new TaskDefinition(
            "periodic_test",
            "Periodic Test Task",
            token =>
            {
                Interlocked.Increment(ref count);
                return Task.CompletedTask;
            },
            interval: TimeSpan.FromMilliseconds(40),
            initialDelay: TimeSpan.Zero
        );

        // Act
        _scheduler.RegisterTask(taskDef);

        // Chờ 150ms (kỳ vọng chạy lần đầu tại 0ms, lần 2 tại 40ms, lần 3 tại 80ms, lần 4 tại 120ms -> ít nhất 3 lần)
        await Task.Delay(150);

        // Assert
        _scheduler.CancelTask("periodic_test");
        var finalCount = Volatile.Read(ref count);
        Assert.True(finalCount >= 3, $"Task định kỳ chỉ chạy {finalCount} lần (kỳ vọng >= 3)");
    }

    [Fact]
    public async Task TriggerNow_ShouldExecuteTaskImmediately()
    {
        // Arrange
        var executed = false;
        var tcs = new TaskCompletionSource<bool>();

        var taskDef = new TaskDefinition(
            "trigger_test",
            "Trigger Test Task",
            token =>
            {
                executed = true;
                tcs.SetResult(true);
                return Task.CompletedTask;
            },
            interval: TimeSpan.FromMinutes(10), // Interval rất lớn để loop chính không tự chạy
            initialDelay: TimeSpan.FromMinutes(10)
        );

        _scheduler.RegisterTask(taskDef);

        // Act
        _scheduler.TriggerNow("trigger_test");

        // Chờ tối đa 500ms
        var completed = await Task.WhenAny(tcs.Task, Task.Delay(500)) == tcs.Task;

        // Assert
        _scheduler.CancelTask("trigger_test");
        Assert.True(completed);
        Assert.True(executed);
    }

    [Fact]
    public async Task CancelTask_ShouldStopPeriodicTask()
    {
        // Arrange
        var count = 0;
        var taskDef = new TaskDefinition(
            "cancel_test",
            "Cancel Test Task",
            token =>
            {
                Interlocked.Increment(ref count);
                return Task.CompletedTask;
            },
            interval: TimeSpan.FromMilliseconds(40),
            initialDelay: TimeSpan.Zero
        );

        _scheduler.RegisterTask(taskDef);
        await Task.Delay(60); // Chạy 1-2 lần
        Assert.True(Volatile.Read(ref count) > 0);

        // Act
        _scheduler.CancelTask("cancel_test");
        var countAfterCancel = Volatile.Read(ref count);

        // Chờ tiếp 100ms nữa để xem task có tăng count tiếp hay không
        await Task.Delay(100);

        // Assert
        Assert.Equal(countAfterCancel, Volatile.Read(ref count));
    }

    // Mock Logger phục vụ test
    private class MockLogger : IStructuredLogger
    {
        public void LogDebug(string message, Dictionary<string, object>? context = null) { }
        public void LogInfo(string message, Dictionary<string, object>? context = null) { }
        public void LogWarning(string message, Dictionary<string, object>? context = null) { }
        public void LogError(string message, Exception? exception = null, Dictionary<string, object>? context = null) { }
    }
}
