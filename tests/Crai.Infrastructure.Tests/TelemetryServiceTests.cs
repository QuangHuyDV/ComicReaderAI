using System;
using System.Collections.Generic;
using System.Threading;
using Xunit;
using Crai.Application.Contracts.Infrastructure;
using Crai.Infrastructure.Telemetry;

namespace Crai.Infrastructure.Tests;

public class TelemetryServiceTests
{
    private readonly InMemoryTelemetryService _telemetryService;
    private readonly MockLogger _mockLogger;

    public TelemetryServiceTests()
    {
        _mockLogger = new MockLogger();
        _telemetryService = new InMemoryTelemetryService(_mockLogger);
    }

    [Fact]
    public void RecordMetric_ShouldStoreMetricValue()
    {
        // Arrange
        var metricName = "AppFps";
        var tags = new Dictionary<string, string> { { "Environment", "Test" } };

        // Act
        _telemetryService.RecordMetric(metricName, 60.0, tags);

        // Assert
        var values = _telemetryService.GetMetricValues(metricName);
        Assert.NotNull(values);
        Assert.Single(values);
        Assert.Equal(60.0, values[0]);
    }

    [Fact]
    public void RecordEvent_ShouldStoreEventAndProperties()
    {
        // Arrange
        var eventName = "OcrProcessed";
        var properties = new Dictionary<string, object>
        {
            { "Language", "en-US" },
            { "CharactersCount", 18 }
        };

        // Act
        _telemetryService.RecordEvent(eventName, properties);

        // Assert
        var events = _telemetryService.GetEvents(eventName);
        Assert.NotNull(events);
        Assert.Single(events);
        Assert.Equal("en-US", events[0]["Language"]);
        Assert.Equal(18, events[0]["CharactersCount"]);
    }

    [Fact]
    public void StartTrace_ShouldMeasureLatency_AndCreateMetric()
    {
        // Arrange
        var traceName = "CaptureDelay";

        // Act
        using (var span = _telemetryService.StartTrace(traceName))
        {
            Thread.Sleep(80); // Giả lập tiến trình tốn 80ms
        }

        // Assert
        var latencyMetricName = $"{traceName}_LatencyMs";
        var values = _telemetryService.GetMetricValues(latencyMetricName);
        
        Assert.NotNull(values);
        Assert.Single(values);
        
        var latency = values[0];
        Assert.True(latency >= 70, $"Latency đo được quá nhỏ: {latency} ms (kỳ vọng >= 70ms)");
        Assert.True(latency < 500, $"Latency đo được quá lớn: {latency} ms (kỳ vọng < 500ms)");
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
