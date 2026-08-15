using System;
using System.Collections.Generic;

namespace Crai.Application.Contracts.Infrastructure;

public interface IStructuredLogger
{
    /// <summary>
    /// Log thông tin debug (chỉ hiển thị ở chế độ phát triển).
    /// </summary>
    void LogDebug(string message, Dictionary<string, object>? context = null);

    /// <summary>
    /// Log thông tin chung (General information).
    /// </summary>
    void LogInfo(string message, Dictionary<string, object>? context = null);

    /// <summary>
    /// Log cảnh báo (Warnings).
    /// </summary>
    void LogWarning(string message, Dictionary<string, object>? context = null);

    /// <summary>
    /// Log lỗi hệ thống (Errors/Exceptions).
    /// </summary>
    void LogError(string message, Exception? exception = null, Dictionary<string, object>? context = null);
}
