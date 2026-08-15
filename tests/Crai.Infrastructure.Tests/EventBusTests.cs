using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Crai.Application.Contracts.Infrastructure;
using Crai.Infrastructure.EventBus;

namespace Crai.Infrastructure.Tests;

public class EventBusTests
{
    private readonly IEventBus _eventBus;
    private readonly MockLogger _mockLogger;

    public EventBusTests()
    {
        _mockLogger = new MockLogger();
        _eventBus = new InMemoryEventBus(_mockLogger);
    }

    [Fact]
    public async Task PublishAsync_ShouldInvokeSubscribedHandlers()
    {
        // Arrange
        var handler1 = new CounterEventHandler();
        var handler2 = new CounterEventHandler();
        _eventBus.Subscribe(handler1);
        _eventBus.Subscribe(handler2);

        var @event = new TestEvent("Hello Event Bus");

        // Act
        await _eventBus.PublishAsync(@event);

        // Assert
        Assert.Equal(1, handler1.Count);
        Assert.Equal(1, handler2.Count);
        Assert.Equal("Hello Event Bus", handler1.LastPayload);
        Assert.Equal("Hello Event Bus", handler2.LastPayload);
    }

    [Fact]
    public async Task Unsubscribe_ShouldStopInvokingHandler()
    {
        // Arrange
        var handler = new CounterEventHandler();
        _eventBus.Subscribe(handler);
        
        var event1 = new TestEvent("First");
        await _eventBus.PublishAsync(event1);
        Assert.Equal(1, handler.Count);

        // Act
        _eventBus.Unsubscribe(handler);
        var event2 = new TestEvent("Second");
        await _eventBus.PublishAsync(event2);

        // Assert
        Assert.Equal(1, handler.Count); // Vẫn giữ nguyên 1, không tăng lên 2
    }

    [Fact]
    public async Task PublishAsync_ShouldIsolateHandlerExceptions()
    {
        // Arrange
        var normalHandler = new CounterEventHandler();
        var failingHandler = new FailingEventHandler();
        
        _eventBus.Subscribe(failingHandler);
        _eventBus.Subscribe(normalHandler);

        var @event = new TestEvent("Test Isolation");

        // Act
        // Hàm này không được quăng exception ra ngoài mà phải xử lý nội bộ và log
        var exception = await Record.ExceptionAsync(() => _eventBus.PublishAsync(@event));

        // Assert
        Assert.Null(exception); // Không có exception nào bắn ra ngoài
        Assert.Equal(1, normalHandler.Count); // Normal handler vẫn chạy thành công
        Assert.True(_mockLogger.ErrorsLogged.Count > 0); // Lỗi của failing handler được ghi nhận trong logger
    }

    // Các class phục vụ Test
    public class TestEvent : ICraiEvent
    {
        public Guid Id { get; } = Guid.NewGuid();
        public DateTime OccurredAt { get; } = DateTime.UtcNow;
        public string Payload { get; }

        public TestEvent(string payload)
        {
            Payload = payload;
        }
    }

    public class CounterEventHandler : IEventHandler<TestEvent>
    {
        public int Count { get; private set; }
        public string? LastPayload { get; private set; }

        public Task HandleAsync(TestEvent @event)
        {
            Count++;
            LastPayload = @event.Payload;
            return Task.CompletedTask;
        }
    }

    public class FailingEventHandler : IEventHandler<TestEvent>
    {
        public Task HandleAsync(TestEvent @event)
        {
            throw new InvalidOperationException("Cố tình quăng lỗi ở Test Handler");
        }
    }

    public class MockLogger : IStructuredLogger
    {
        public List<string> ErrorsLogged { get; } = new();

        public void LogDebug(string message, Dictionary<string, object>? context = null) { }
        public void LogInfo(string message, Dictionary<string, object>? context = null) { }
        public void LogWarning(string message, Dictionary<string, object>? context = null) { }

        public void LogError(string message, Exception? exception = null, Dictionary<string, object>? context = null)
        {
            ErrorsLogged.Add(message);
        }
    }
}
