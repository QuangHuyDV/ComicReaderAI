using System;
using System.IO;
using System.Collections.Generic;
using Xunit;
using Crai.Application.Contracts.Infrastructure;
using Crai.Infrastructure.Secret;

namespace Crai.Infrastructure.Tests;

public class SecretManagerTests : IDisposable
{
    private readonly string _secretsFileName;
    private readonly string _secretsFullPath;
    private readonly MockLogger _mockLogger;

    public SecretManagerTests()
    {
        _secretsFileName = $"secrets_test_{Guid.NewGuid()}.dat";
        _secretsFullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _secretsFileName);
        _mockLogger = new MockLogger();
    }

    public void Dispose()
    {
        if (File.Exists(_secretsFullPath))
        {
            try
            {
                File.Delete(_secretsFullPath);
            }
            catch
            {
                // Bỏ qua lỗi lock file khi xóa file test
            }
        }
    }

    [Fact]
    public void StoreAndGetSecret_ShouldEncryptAndDecryptCorrectly()
    {
        // Arrange
        var secretManager = new DpapiSecretManager(_mockLogger, _secretsFileName);
        var key = "GeminiApiKey";
        var originalValue = "AIzaSyTestApiKey_123456";

        // Act
        secretManager.StoreSecret(key, originalValue);
        var decryptedValue = secretManager.GetSecret(key);

        // Assert
        Assert.Equal(originalValue, decryptedValue); // Giải mã thành công và trùng khớp

        // Kiểm tra tính bảo mật: Đọc file thô từ disk để xác thực giá trị không bị lưu plain-text
        Assert.True(File.Exists(_secretsFullPath));
        var rawContent = File.ReadAllText(_secretsFullPath);
        
        Assert.DoesNotContain(originalValue, rawContent); // Tuyệt đối không chứa text gốc chưa mã hóa
        Assert.Contains(key, rawContent); // Chứa key định danh dạng thô để map
    }

    [Fact]
    public void StoreSecret_ShouldOverwriteExistingSecret()
    {
        // Arrange
        var secretManager = new DpapiSecretManager(_mockLogger, _secretsFileName);
        var key = "TestKey";
        
        // Act - Ghi đè
        secretManager.StoreSecret(key, "FirstValue");
        secretManager.StoreSecret(key, "SecondValue");
        
        var decryptedValue = secretManager.GetSecret(key);

        // Assert
        Assert.Equal("SecondValue", decryptedValue);
    }

    [Fact]
    public void RemoveSecret_ShouldDeleteSecretSuccessfully()
    {
        // Arrange
        var secretManager = new DpapiSecretManager(_mockLogger, _secretsFileName);
        var key = "ErasableKey";
        secretManager.StoreSecret(key, "ToBeDeleted");
        Assert.Equal("ToBeDeleted", secretManager.GetSecret(key));

        // Act
        secretManager.RemoveSecret(key);
        var decryptedValue = secretManager.GetSecret(key);

        // Assert
        Assert.Null(decryptedValue); // Secret đã bị xóa hoàn toàn
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
