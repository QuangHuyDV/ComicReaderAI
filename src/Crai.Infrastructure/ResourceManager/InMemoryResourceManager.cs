using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Crai.Application.Contracts.Infrastructure;

namespace Crai.Infrastructure.ResourceManager;

public class InMemoryResourceManager : IResourceManager
{
    private readonly ConcurrentDictionary<string, ResourceRegistry> _registry = new();
    private readonly IStructuredLogger _logger;
    private bool _isShutdown;

    public InMemoryResourceManager(IStructuredLogger logger)
    {
        _logger = logger;
    }

    public void RegisterResource<T>(string id, string name, Func<T> factory) where T : class
    {
        if (_isShutdown)
        {
            _logger.LogWarning($"[ResourceManager] Không thể đăng ký tài nguyên '{name}' do hệ thống đã shutdown.");
            return;
        }

        var descriptor = new ResourceDescriptor(id, name, typeof(T));
        var registry = new ResourceRegistry(descriptor, () => factory());

        if (!_registry.TryAdd(id, registry))
        {
            _logger.LogWarning($"[ResourceManager] Tài nguyên Id '{id}' đã tồn tại trong registry. Bỏ qua đăng ký mới.");
        }
        else
        {
            _logger.LogDebug($"[ResourceManager] Đăng ký thành công tài nguyên: {name} (Id: {id})");
        }
    }

    public IResourceLease<T> Acquire<T>(string id) where T : class
    {
        if (_isShutdown)
        {
            throw new InvalidOperationException("[ResourceManager] Không thể acquire tài nguyên do hệ thống đã shutdown.");
        }

        if (!_registry.TryGetValue(id, out var registry))
        {
            throw new KeyNotFoundException($"[ResourceManager] Không tìm thấy tài nguyên nào có Id '{id}' đăng ký trong hệ thống.");
        }

        lock (registry)
        {
            // Khởi tạo lazy instance nếu là lần đầu tiên gọi
            if (registry.Instance == null)
            {
                _logger.LogDebug($"[ResourceManager] Đang khởi tạo tài nguyên '{registry.Descriptor.Name}' (Id: {id}) qua Factory...");
                registry.Instance = registry.Factory();
            }

            registry.ReferenceCount++;
            _logger.LogDebug($"[ResourceManager] Thuê (Acquired) '{registry.Descriptor.Name}' (Id: {id}). RefCount: {registry.ReferenceCount}");

            if (registry.Instance is not T typedInstance)
            {
                // Giảm ref count do acquire thất bại
                registry.ReferenceCount--;
                throw new InvalidCastException($"[ResourceManager] Kiểu tài nguyên thực tế '{registry.Instance.GetType().Name}' không thể ép kiểu sang '{typeof(T).Name}'.");
            }

            return new ResourceLease<T>(id, typedInstance, lease => Release(lease));
        }
    }

    public void Release<T>(IResourceLease<T> lease) where T : class
    {
        if (!_registry.TryGetValue(lease.ResourceId, out var registry))
        {
            return;
        }

        lock (registry)
        {
            if (registry.ReferenceCount > 0)
            {
                registry.ReferenceCount--;
                _logger.LogDebug($"[ResourceManager] Trả (Released) '{registry.Descriptor.Name}' (Id: {lease.ResourceId}). RefCount: {registry.ReferenceCount}");

                // Giải phóng tài nguyên khi không còn ai sử dụng (RefCount == 0)
                if (registry.ReferenceCount == 0 && registry.Instance != null)
                {
                    _logger.LogDebug($"[ResourceManager] RefCount về 0. Đang giải phóng tài nguyên '{registry.Descriptor.Name}' (Id: {lease.ResourceId})...");
                    if (registry.Instance is IDisposable disposable)
                    {
                        try
                        {
                            disposable.Dispose();
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"[ResourceManager] Lỗi khi Dispose tài nguyên '{registry.Descriptor.Name}': {ex.Message}", ex);
                        }
                    }
                    registry.Instance = null; // GC dọn dẹp
                }
            }
        }
    }

    public void Shutdown()
    {
        if (_isShutdown) return;
        _isShutdown = true;

        _logger.LogInfo("[ResourceManager] Đang tắt hệ thống Resource Manager, giải phóng mọi tài nguyên hoạt động...");

        foreach (var key in _registry.Keys.ToList())
        {
            if (_registry.TryRemove(key, out var registry))
            {
                lock (registry)
                {
                    if (registry.Instance != null)
                    {
                        if (registry.Instance is IDisposable disposable)
                        {
                            try
                            {
                                disposable.Dispose();
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError($"[ResourceManager] Lỗi khi Shutdown và Dispose tài nguyên '{registry.Descriptor.Name}': {ex.Message}", ex);
                            }
                        }
                        registry.Instance = null;
                    }
                    registry.ReferenceCount = 0;
                }
            }
        }

        _logger.LogInfo("[ResourceManager] Giải phóng tài nguyên hoàn tất.");
    }

    // Helper kiểm tra ReferenceCount phục vụ Unit Test
    public int GetReferenceCount(string id)
    {
        if (_registry.TryGetValue(id, out var registry))
        {
            lock (registry)
            {
                return registry.ReferenceCount;
            }
        }
        return -1;
    }

    private class ResourceRegistry
    {
        public ResourceDescriptor Descriptor { get; }
        public Func<object> Factory { get; }
        public object? Instance { get; set; }
        public int ReferenceCount { get; set; }

        public ResourceRegistry(ResourceDescriptor descriptor, Func<object> factory)
        {
            Descriptor = descriptor;
            Factory = factory;
        }
    }
}
