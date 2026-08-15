using System;
using System.Collections.Generic;
using Xunit;
using Crai.Application.Contracts.Infrastructure;
using Crai.Infrastructure.ResourceManager;

namespace Crai.Infrastructure.Tests;

public class ResourceManagerTests
{
    private readonly InMemoryResourceManager _resourceManager;
    private readonly MockLogger _mockLogger;

    public ResourceManagerTests()
    {
        _mockLogger = new MockLogger();
        _resourceManager = new InMemoryResourceManager(_mockLogger);
    }

    [Fact]
    public void RegisterResource_ShouldNotInstantiateImmediately()
    {
        // Arrange
        var factoryCalled = false;
        _resourceManager.RegisterResource("lazy_res", "Lazy Test Resource", () =>
        {
            factoryCalled = true;
            return new DummyResource();
        });

        // Assert
        Assert.False(factoryCalled); // Factory chưa được gọi tại bước đăng ký
        Assert.Equal(0, _resourceManager.GetReferenceCount("lazy_res"));
    }

    [Fact]
    public void Acquire_ShouldCreateInstanceAndIncrementRefCount()
    {
        // Arrange
        _resourceManager.RegisterResource("counter_res", "Counter Test Resource", () => new DummyResource());

        // Act
        using (var lease1 = _resourceManager.Acquire<DummyResource>("counter_res"))
        {
            // Assert
            Assert.NotNull(lease1.Resource);
            Assert.Equal(1, _resourceManager.GetReferenceCount("counter_res"));

            using (var lease2 = _resourceManager.Acquire<DummyResource>("counter_res"))
            {
                // Assert
                Assert.Same(lease1.Resource, lease2.Resource); // Trả về cùng một instance dùng chung
                Assert.Equal(2, _resourceManager.GetReferenceCount("counter_res"));
            }

            // Assert sau khi lease2 bị dispose (release)
            Assert.Equal(1, _resourceManager.GetReferenceCount("counter_res"));
        }

        // Assert sau khi tất cả leases bị dispose (release)
        Assert.Equal(0, _resourceManager.GetReferenceCount("counter_res"));
    }

    [Fact]
    public void Release_ShouldDisposeInstance_WhenRefCountHitsZero()
    {
        // Arrange
        var resourceInstance = new DisposableResource();
        _resourceManager.RegisterResource("disposable_res", "Disposable Resource", () => resourceInstance);

        // Act
        var lease = _resourceManager.Acquire<DisposableResource>("disposable_res");
        Assert.False(resourceInstance.IsDisposed);

        // Act - Release lease
        lease.Dispose();

        // Assert
        Assert.True(resourceInstance.IsDisposed); // Resource bị Dispose tự động khi ref count về 0
        Assert.Equal(0, _resourceManager.GetReferenceCount("disposable_res"));
    }

    [Fact]
    public void Shutdown_ShouldDisposeAllActiveInstances()
    {
        // Arrange
        var res1 = new DisposableResource();
        var res2 = new DisposableResource();

        _resourceManager.RegisterResource("res1", "Resource 1", () => res1);
        _resourceManager.RegisterResource("res2", "Resource 2", () => res2);

        // Acquire để khởi tạo instances
        var lease1 = _resourceManager.Acquire<DisposableResource>("res1");
        var lease2 = _resourceManager.Acquire<DisposableResource>("res2");

        Assert.False(res1.IsDisposed);
        Assert.False(res2.IsDisposed);

        // Act
        _resourceManager.Shutdown();

        // Assert
        Assert.True(res1.IsDisposed); // Đã bị dispose cưỡng chế
        Assert.True(res2.IsDisposed); // Đã bị dispose cưỡng chế
    }

    // Các class phục vụ Test
    public class DummyResource
    {
        public string Data { get; } = "Some Data";
    }

    public class DisposableResource : IDisposable
    {
        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }

    // Mock Logger phục vụ test
    private class MockLogger : IStructuredLogger
    {
        public void LogDebug(string message, Dictionary<string, object>? context = null) { }
        public void LogInfo(string message, Dictionary<string, object>? context = null) { }
        public void LogWarning(string message, Dictionary<string, object>? context = null) { }
        public void LogError(string message, Exception? exception = null, Dictionary<string, object>? context = null) { }
    }
}
