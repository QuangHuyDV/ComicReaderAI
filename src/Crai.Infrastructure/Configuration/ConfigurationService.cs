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

    public void UpdateValue(string key, object value)
    {
        try
        {
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var fullPath = Path.Combine(basePath, _configFilePath);

            string jsonContent = File.Exists(fullPath) ? File.ReadAllText(fullPath) : "{}";
            var rootNode = System.Text.Json.Nodes.JsonNode.Parse(jsonContent);
            if (rootNode == null)
            {
                rootNode = new System.Text.Json.Nodes.JsonObject();
            }

            var parts = key.Split(':');
            System.Text.Json.Nodes.JsonNode? currentNode = rootNode;

            for (int i = 0; i < parts.Length - 1; i++)
            {
                var part = parts[i];
                var nextNode = currentNode[part];
                if (nextNode == null)
                {
                    var newObj = new System.Text.Json.Nodes.JsonObject();
                    currentNode[part] = newObj;
                    currentNode = newObj;
                }
                else
                {
                    currentNode = nextNode;
                }
            }

            var lastPart = parts[parts.Length - 1];
            if (value is bool bValue)
            {
                currentNode[lastPart] = bValue;
            }
            else if (value is int iValue)
            {
                currentNode[lastPart] = iValue;
            }
            else if (value is double dValue)
            {
                currentNode[lastPart] = dValue;
            }
            else if (value is float fValue)
            {
                currentNode[lastPart] = fValue;
            }
            else
            {
                currentNode[lastPart] = value?.ToString();
            }

            var options = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
            string updatedJson = rootNode.ToJsonString(options);
            File.WriteAllText(fullPath, updatedJson);

            Reload();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Config] Lỗi cập nhật cấu hình cho key '{key}': {ex.Message}");
        }
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
    ""GeminiApiKey"": """",
    ""MergeLines"": true,
    ""OverlayDuration"": 8,
    ""Continuous"": false,
    ""ContinuousDelayMs"": 1000
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
