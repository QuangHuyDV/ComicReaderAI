using System;
using System.IO;
using System.Collections.Generic;
using Serilog;
using Serilog.Formatting.Compact;
using Crai.Application.Contracts.Infrastructure;

namespace Crai.Infrastructure.Logging;

public class StructuredLogger : IStructuredLogger
{
    private static string GetAppDataDirectory()
    {
        bool isTest = false;
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var name = assembly.FullName;
            if (name != null && (name.Contains("xunit", StringComparison.OrdinalIgnoreCase) || 
                                 name.Contains("test", StringComparison.OrdinalIgnoreCase)))
            {
                isTest = true;
                break;
            }
        }

        if (isTest)
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var craiDir = Path.Combine(appData, "Crai");
        if (!Directory.Exists(craiDir))
        {
            Directory.CreateDirectory(craiDir);
        }
        return craiDir;
    }

    static StructuredLogger()
    {
        var logDir = Path.Combine(GetAppDataDirectory(), "logs");
        if (!Directory.Exists(logDir))
        {
            Directory.CreateDirectory(logDir);
        }

        var logFilePath = Path.Combine(logDir, "crai_log.json");

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            // Log Console đẹp mắt cho phát triển
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
            // Log File dạng JSON cấu trúc gọn gàng cho sản xuất
            .WriteTo.File(new CompactJsonFormatter(), logFilePath, rollingInterval: RollingInterval.Day)
            .CreateLogger();
    }

    private static Serilog.ILogger GetLoggerWithContext(Dictionary<string, object>? context)
    {
        var logger = Log.Logger;
        if (context != null)
        {
            foreach (var kvp in context)
            {
                logger = logger.ForContext(kvp.Key, kvp.Value);
            }
        }
        return logger;
    }

    public void LogDebug(string message, Dictionary<string, object>? context = null)
    {
        GetLoggerWithContext(context).Debug(message);
    }

    public void LogInfo(string message, Dictionary<string, object>? context = null)
    {
        GetLoggerWithContext(context).Information(message);
    }

    public void LogWarning(string message, Dictionary<string, object>? context = null)
    {
        GetLoggerWithContext(context).Warning(message);
    }

    public void LogError(string message, Exception? exception = null, Dictionary<string, object>? context = null)
    {
        if (exception != null)
        {
            GetLoggerWithContext(context).Error(exception, message);
        }
        else
        {
            GetLoggerWithContext(context).Error(message);
        }
    }
}
