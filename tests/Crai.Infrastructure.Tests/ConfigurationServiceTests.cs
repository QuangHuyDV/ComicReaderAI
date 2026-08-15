using System;
using System.IO;
using System.Threading;
using Xunit;
using Crai.Infrastructure.Configuration;

namespace Crai.Infrastructure.Tests;

public class ConfigurationServiceTests : IDisposable
{
    private readonly string _testConfigFileName;
    private readonly string _testConfigFullPath;

    public ConfigurationServiceTests()
    {
        _testConfigFileName = $"appsettings_test_{Guid.NewGuid()}.json";
        _testConfigFullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _testConfigFileName);
    }

    public void Dispose()
    {
        if (File.Exists(_testConfigFullPath))
        {
            try
            {
                File.Delete(_testConfigFullPath);
            }
            catch
            {
                // Bỏ qua nếu file đang bị lock
            }
        }
    }

    [Fact]
    public void Constructor_ShouldCreateDefaultFile_WhenFileDoesNotExist()
    {
        // Arrange & Act
        var configService = new ConfigurationService(_testConfigFileName);

        // Assert
        Assert.True(File.Exists(_testConfigFullPath));
        var theme = configService.GetValue<string>("App:Theme");
        Assert.Equal("Dark", theme);
    }

    [Fact]
    public void GetValue_ShouldReturnExpectedSettings()
    {
        // Arrange
        var jsonContent = @"{
  ""TestKey"": ""HelloTest"",
  ""Nested"": {
    ""Number"": 42
  }
}";
        File.WriteAllText(_testConfigFullPath, jsonContent);

        // Act
        var configService = new ConfigurationService(_testConfigFileName);

        // Assert
        Assert.Equal("HelloTest", configService.GetValue<string>("TestKey"));
        Assert.Equal(42, configService.GetValue<int>("Nested:Number"));
    }

    [Fact]
    public void GetSection_ShouldBindToPocoClass()
    {
        // Arrange
        var jsonContent = @"{
  ""OCR"": {
    ""Engine"": ""WindowsMedia"",
    ""DefaultLanguage"": ""zh-CN""
  }
}";
        File.WriteAllText(_testConfigFullPath, jsonContent);
        var configService = new ConfigurationService(_testConfigFileName);

        // Act
        var ocrSettings = configService.GetSection<TestOcrSettings>("OCR");

        // Assert
        Assert.NotNull(ocrSettings);
        Assert.Equal("WindowsMedia", ocrSettings.Engine);
        Assert.Equal("zh-CN", ocrSettings.DefaultLanguage);
    }

    [Fact]
    public void HotReload_ShouldApplyChangesAutomatically_WhenFileIsModified()
    {
        // Arrange
        var initialJson = @"{
  ""DynamicKey"": ""InitialValue""
}";
        File.WriteAllText(_testConfigFullPath, initialJson);
        var configService = new ConfigurationService(_testConfigFileName);
        Assert.Equal("InitialValue", configService.GetValue<string>("DynamicKey"));

        // Act - Thay đổi file cấu hình trên đĩa
        var modifiedJson = @"{
  ""DynamicKey"": ""NewValue""
}";
        File.WriteAllText(_testConfigFullPath, modifiedJson);

        // Poll chờ giá trị thay đổi (tối đa 2.5 giây, break ngay khi có giá trị mới)
        string? newValue = null;
        for (int i = 0; i < 25; i++)
        {
            newValue = configService.GetValue<string>("DynamicKey");
            if (newValue == "NewValue")
                break;
            Thread.Sleep(100);
        }

        // Assert
        Assert.Equal("NewValue", newValue);
    }

    public class TestOcrSettings
    {
        public string Engine { get; set; } = string.Empty;
        public string DefaultLanguage { get; set; } = string.Empty;
    }
}
