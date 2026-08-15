using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Crai.Application.Contracts.Infrastructure;

namespace Crai.Infrastructure.Telemetry;

public class InMemoryTelemetryService : ITelemetryService
{
    private readonly ConcurrentDictionary<string, List<double>> _metrics = new();
    private readonly ConcurrentDictionary<string, List<Dictionary<string, object>>> _events = new();
    private readonly IStructuredLogger _logger;

    public InMemoryTelemetryService(IStructuredLogger logger)
    {
        _logger = logger;
    }

    public void RecordMetric(string name, double value, Dictionary<string, string>? tags = null)
    {
        _metrics.AddOrUpdate(name,
            _ => new List<double> { value },
            (_, list) =>
            {
                lock (list)
                {
                    list.Add(value);
                }
                return list;
            });

        var context = tags != null
            ? tags.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value)
            : new Dictionary<string, object>();

        _logger.LogDebug($"[Telemetry] Metric '{name}': {value}", context);
    }

    public void RecordEvent(string name, Dictionary<string, object>? properties = null)
    {
        var props = properties ?? new Dictionary<string, object>();
        _events.AddOrUpdate(name,
            _ => new List<Dictionary<string, object>> { props },
            (_, list) =>
            {
                lock (list)
                {
                    list.Add(props);
                }
                return list;
            });

        _logger.LogInfo($"[Telemetry] Event '{name}'", props);
    }

    public ITraceSpan StartTrace(string name)
    {
        _logger.LogDebug($"[Telemetry] Bắt đầu trace span '{name}'");
        return new TelemetryTraceSpan(name, (spanName, elapsedMs) =>
        {
            _logger.LogInfo($"[Telemetry] Kết thúc trace span '{spanName}' in {elapsedMs} ms",
                new Dictionary<string, object> { { "SpanName", spanName }, { "ElapsedMs", elapsedMs } });

            // Tự động ghi nhận metric latency
            RecordMetric($"{spanName}_LatencyMs", elapsedMs);
        });
    }

    // Hỗ trợ Unit Test truy xuất metrics
    public IReadOnlyList<double>? GetMetricValues(string name)
    {
        if (_metrics.TryGetValue(name, out var list))
        {
            lock (list)
            {
                return list.ToArray();
            }
        }
        return null;
    }

    // Hỗ trợ Unit Test truy xuất events
    public IReadOnlyList<Dictionary<string, object>>? GetEvents(string name)
    {
        if (_events.TryGetValue(name, out var list))
        {
            lock (list)
            {
                return list.ToArray();
            }
        }
        return null;
    }
}
