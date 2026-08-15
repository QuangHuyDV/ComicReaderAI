using System;
using System.Diagnostics;
using Crai.Application.Contracts.Infrastructure;

namespace Crai.Infrastructure.Telemetry;

public class TelemetryTraceSpan : ITraceSpan
{
    private readonly Stopwatch _stopwatch;
    private readonly Action<string, long> _onCompleted;
    private bool _disposed;

    public string Name { get; }
    public long ElapsedMilliseconds => _stopwatch.ElapsedMilliseconds;

    public TelemetryTraceSpan(string name, Action<string, long> onCompleted)
    {
        Name = name;
        _onCompleted = onCompleted;
        _stopwatch = Stopwatch.StartNew();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _stopwatch.Stop();
        _onCompleted(Name, _stopwatch.ElapsedMilliseconds);
        _disposed = true;
    }
}
