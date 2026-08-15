using System;

namespace Crai.Application.Contracts.Infrastructure;

public interface IResourceLease<out T> : IDisposable
{
    /// <summary>
    /// Mã định danh của tài nguyên được thuê (leased).
    /// </summary>
    string ResourceId { get; }

    /// <summary>
    /// Đối tượng tài nguyên thực tế.
    /// </summary>
    T Resource { get; }
}
