using System;

namespace Crai.Application.Contracts.Infrastructure;

public interface IResourceManager
{
    /// <summary>
    /// Đăng ký một tài nguyên vào hệ thống kèm factory để khởi tạo lười (lazy initialization).
    /// </summary>
    void RegisterResource<T>(string id, string name, Func<T> factory) where T : class;

    /// <summary>
    /// Thuê tài nguyên theo Id. Tài nguyên sẽ tự động được khởi tạo ở lần gọi đầu tiên.
    /// Tự động tăng reference counter cho tài nguyên.
    /// </summary>
    IResourceLease<T> Acquire<T>(string id) where T : class;

    /// <summary>
    /// Trả lại tài nguyên và giảm reference counter.
    /// Khi ref counter về 0, tài nguyên sẽ tự động được giải phóng (nếu implement IDisposable).
    /// </summary>
    void Release<T>(IResourceLease<T> lease) where T : class;

    /// <summary>
    /// Dọn dẹp và giải phóng toàn bộ tài nguyên trong hệ thống.
    /// </summary>
    void Shutdown();
}
