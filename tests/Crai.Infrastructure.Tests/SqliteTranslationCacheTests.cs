using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Crai.Application.Contracts.Infrastructure;
using Crai.Modules.Storage.Services;

namespace Crai.Infrastructure.Tests;

public class SqliteTranslationCacheTests : IDisposable
{
    private readonly string _dbFileName;
    private readonly string _dbFullPath;
    private readonly MockLogger _mockLogger;

    public SqliteTranslationCacheTests()
    {
        _dbFileName = $"test_cache_{Guid.NewGuid()}.db";
        _dbFullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _dbFileName);
        _mockLogger = new MockLogger();
    }

    public void Dispose()
    {
        // Giải phóng file DB sau khi test xong
        if (File.Exists(_dbFullPath))
        {
            try
            {
                // Gọi dọn dẹp SQLite engine trước khi xóa file vật lý
                Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
                File.Delete(_dbFullPath);
            }
            catch
            {
                // Bỏ qua lỗi lock file khi delete
            }
        }
    }

    [Fact]
    public async Task SetAndGet_ShouldCacheCorrectly()
    {
        // Arrange
        var cache = new SqliteTranslationCache(_mockLogger, _dbFileName);
        var source = "Hello, how are you?";
        var targetLang = "vi";
        var expectedTranslation = "Xin chào, bạn khỏe không?";

        // Act
        await cache.SetAsync(source, targetLang, expectedTranslation);
        var actualTranslation = await cache.GetAsync(source, targetLang);

        // Assert
        Assert.Equal(expectedTranslation, actualTranslation);
    }

    [Fact]
    public async Task Set_ShouldOverwriteExistingCache()
    {
        // Arrange
        var cache = new SqliteTranslationCache(_mockLogger, _dbFileName);
        var source = "Overwritable";
        var targetLang = "vi";

        // Act
        await cache.SetAsync(source, targetLang, "First Translation");
        await cache.SetAsync(source, targetLang, "Second Translation");
        var actualTranslation = await cache.GetAsync(source, targetLang);

        // Assert
        Assert.Equal("Second Translation", actualTranslation);
    }

    [Fact]
    public async Task Clear_ShouldEmptyCache()
    {
        // Arrange
        var cache = new SqliteTranslationCache(_mockLogger, _dbFileName);
        var source = "ClearMe";
        var targetLang = "vi";
        await cache.SetAsync(source, targetLang, "To Be Cleared");

        // Act
        await cache.ClearAsync();
        var actualTranslation = await cache.GetAsync(source, targetLang);

        // Assert
        Assert.Null(actualTranslation);
    }

    [Fact]
    public async Task Get_ShouldReturnNull_WhenCacheMiss()
    {
        // Arrange
        var cache = new SqliteTranslationCache(_mockLogger, _dbFileName);

        // Act
        var result = await cache.GetAsync("NonExistentText", "vi");

        // Assert
        Assert.Null(result);
    }

    private class MockLogger : IStructuredLogger
    {
        public void LogDebug(string message, Dictionary<string, object>? context = null) { }
        public void LogInfo(string message, Dictionary<string, object>? context = null) { }
        public void LogWarning(string message, Dictionary<string, object>? context = null) { }
        public void LogError(string message, Exception? exception = null, Dictionary<string, object>? context = null) { }
    }
}
