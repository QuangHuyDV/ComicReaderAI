using System.Collections.Generic;

namespace Crai.Application.Contracts.Infrastructure;

public interface ITelemetryService
{
    /// <summary>
    /// Ghi nhận một chỉ số đo lường (metric) dạng số (ví dụ: CPU usage, Memory size, FPS).
    /// </summary>
    void RecordMetric(string name, double value, Dictionary<string, string>? tags = null);

    /// <summary>
    /// Ghi nhận một sự kiện nghiệp vụ (telemetry event) kèm dữ liệu mô tả (properties).
    /// </summary>
    void RecordEvent(string name, Dictionary<string, object>? properties = null);

    /// <summary>
    /// Bắt đầu một tiến trình đo thời gian chạy (trace span) giúp phân tích latency.
    /// </summary>
    ITraceSpan StartTrace(string name);
}
