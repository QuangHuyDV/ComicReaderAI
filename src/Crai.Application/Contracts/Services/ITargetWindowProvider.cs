using System;

namespace Crai.Application.Contracts.Services;

public interface ITargetWindowProvider
{
    /// <summary>
    /// Trả về tham chiếu tới Window mục tiêu (dưới dạng object để Application layer độc lập với UI framework).
    /// </summary>
    object? GetTargetWindow();
}
