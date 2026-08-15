using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Crai.Application.Contracts.Infrastructure;

namespace Crai.Infrastructure.Configuration;

public class ConfigurationService : IConfigurationService
{
    private readonly IConfigurationRoot _configuration;
    private readonly string _configFilePath;

    public ConfigurationService(string? configFilePath = null)
    {
        _configFilePath = configFilePath ?? "appsettings.json";
        
        var basePath = AppDomain.CurrentDomain.BaseDirectory;
        var fullPath = Path.Combine(basePath, _configFilePath);

        // Tạo file cấu hình mặc định nếu chưa tồn tại
        if (!File.Exists(fullPath))
        {
            CreateDefaultConfigFile(fullPath);
        }

        var builder = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile(_configFilePath, optional: false, reloadOnChange: true);

        _configuration = builder.Build();
    }

    public T? GetValue<T>(string key)
    {
        try
        {
            return _configuration.GetValue<T>(key);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Config] Lỗi lấy giá trị key '{key}': {ex.Message}");
            return default;
        }
    }

    public T GetSection<T>(string sectionName) where T : class, new()
    {
        var section = _configuration.GetSection(sectionName);
        var obj = new T();
        section.Bind(obj);
        return obj;
    }

    public void Reload()
    {
        _configuration.Reload();
        Console.WriteLine("[Config] Đã tải lại cấu hình thủ công.");
    }

    private static void CreateDefaultConfigFile(string path)
    {
        var defaultJson = @"{
  ""App"": {
    ""Theme"": ""Dark"",
    ""Hotkey"": ""Ctrl+Shift+T""
  },
  ""OCR"": {
    ""Engine"": ""WindowsMedia"",
    ""DefaultLanguage"": ""en-US""
  },
  ""Translation"": {
    ""Engine"": ""GoogleTranslate"",
    ""GeminiApiKey"": """"
  }
}";
        try
        {
            var dir = Path.GetDirectoryName(path);
            if (dir != null && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            File.WriteAllText(path, defaultJson);
            Console.WriteLine($"[Config] Đã khởi tạo file cấu hình mặc định tại: {path}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Config] Không thể tạo file cấu hình mặc định: {ex.Message}");
        }
    }
}
