using System;

namespace Crai.Application.Contracts.Infrastructure;

public interface ICraiEvent
{
    /// <summary>
    /// Mã định danh duy nhất của sự kiện.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// Thời gian sự kiện phát sinh.
    /// </summary>
    DateTime OccurredAt { get; }
}
