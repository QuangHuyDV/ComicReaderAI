using System;

namespace Crai.Application.Contracts.Infrastructure;

public interface ITraceSpan : IDisposable
{
    /// <summary>
    /// Tên của span trace (ví dụ: "CaptureWindow", "OCRProcess").
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Thời gian thực thi đo được tính bằng mili-giây.
    /// </summary>
    long ElapsedMilliseconds { get; }
}
