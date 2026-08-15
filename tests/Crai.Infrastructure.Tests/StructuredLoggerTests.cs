using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using Xunit;
using Crai.Infrastructure.Logging;

namespace Crai.Infrastructure.Tests;

public class StructuredLoggerTests
{
    private readonly string _logDirectory;

    public StructuredLoggerTests()
    {
        _logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
    }

    [Fact]
    public void Logger_ShouldCreateLogDirectoryAndFile_WhenLogsAreWritten()
    {
        // Arrange
        var logger = new StructuredLogger();
        var testMessage = $"Test log entry {Guid.NewGuid()}";
        var context = new Dictionary<string, object>
        {
            { "TestProperty", "TestValue" },
            { "ExecutionId", 99 }
        };

        // Act
        logger.LogInfo(testMessage, context);

        // Chờ 100ms để đảm bảo Serilog File Sink flush dữ liệu xuống disk
        Thread.Sleep(150);

        // Assert
        Assert.True(Directory.Exists(_logDirectory));
        
        var logFiles = Directory.GetFiles(_logDirectory, "*.json");
        Assert.NotEmpty(logFiles);

        // Đọc nội dung file log cuối cùng để tìm chuỗi testMessage
        bool foundMessage = false;
        bool foundProperty = false;

        foreach (var file in logFiles)
        {
            try
            {
                // Dùng FileShare.ReadWrite vì file log đang được Serilog giữ lock để ghi
                using var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = new StreamReader(fs);
                var content = reader.ReadToEnd();
                
                if (content.Contains(testMessage))
                {
                    foundMessage = true;
                }
                if (content.Contains("TestProperty") && content.Contains("TestValue"))
                {
                    foundProperty = true;
                }
            }
            catch
            {
                // Bỏ qua lỗi lock file khi đọc file log
            }
        }

        Assert.True(foundMessage, "Không tìm thấy nội dung log trong file JSON.");
        Assert.True(foundProperty, "Không tìm thấy structured context properties trong file JSON.");
    }
}
