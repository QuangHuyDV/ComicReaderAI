using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Runtime.Versioning;
using Crai.Application.Contracts.Infrastructure;

namespace Crai.Infrastructure.Secret;

[SupportedOSPlatform("windows")]
public class DpapiSecretManager : ISecretManager
{
    private static readonly byte[] Entropy = new byte[] { 83, 101, 99, 114, 101, 116, 67, 114, 97, 105, 75, 101, 121, 50, 48, 50, 54 }; // "SecretCraiKey2026"
    private readonly string _secretsFilePath;
    private readonly object _lock = new();
    private readonly IStructuredLogger _logger;

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

    public DpapiSecretManager(IStructuredLogger logger, string? secretsFileName = null)
    {
        _logger = logger;
        var fileName = secretsFileName ?? "secrets.dat";
        _secretsFilePath = Path.Combine(GetAppDataDirectory(), fileName);
    }

    public void StoreSecret(string key, string secretValue)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Key không được để trống.", nameof(key));
        if (secretValue == null) throw new ArgumentNullException(nameof(secretValue));

        lock (_lock)
        {
            try
            {
                var secrets = LoadSecretsDictionary();

                // 1. Mã hóa giá trị bằng Windows DPAPI (CurrentUser scope)
                var rawBytes = Encoding.UTF8.GetBytes(secretValue);
                var encryptedBytes = ProtectedData.Protect(rawBytes, Entropy, DataProtectionScope.CurrentUser);
                var base64Encrypted = Convert.ToBase64String(encryptedBytes);

                // 2. Cập nhật dictionary và lưu
                secrets[key] = base64Encrypted;
                SaveSecretsDictionary(secrets);

                _logger.LogDebug($"[SecretManager] Đã mã hóa và lưu trữ thành công secret cho key: {key}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"[SecretManager] Không thể lưu trữ secret cho key '{key}': {ex.Message}", ex);
                throw new InvalidOperationException($"Lỗi mã hóa hoặc lưu trữ secret: {ex.Message}", ex);
            }
        }
    }

    public string? GetSecret(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return null;

        lock (_lock)
        {
            try
            {
                var secrets = LoadSecretsDictionary();
                if (!secrets.TryGetValue(key, out var base64Encrypted))
                {
                    return null;
                }

                // 1. Giải mã bằng Windows DPAPI
                var encryptedBytes = Convert.FromBase64String(base64Encrypted);
                var decryptedBytes = ProtectedData.Unprotect(encryptedBytes, Entropy, DataProtectionScope.CurrentUser);

                return Encoding.UTF8.GetString(decryptedBytes);
            }
            catch (CryptographicException ex)
            {
                _logger.LogError($"[SecretManager] Lỗi giải mã DPAPI cho key '{key}'. Có thể dữ liệu bị hỏng hoặc được tạo từ máy khác: {ex.Message}", ex);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[SecretManager] Lỗi không mong đợi khi truy xuất secret cho key '{key}': {ex.Message}", ex);
                return null;
            }
        }
    }

    public void RemoveSecret(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return;

        lock (_lock)
        {
            try
            {
                var secrets = LoadSecretsDictionary();
                if (secrets.Remove(key))
                {
                    SaveSecretsDictionary(secrets);
                    _logger.LogDebug($"[SecretManager] Đã xóa thành công secret cho key: {key}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"[SecretManager] Lỗi khi xóa secret cho key '{key}': {ex.Message}", ex);
            }
        }
    }

    private Dictionary<string, string> LoadSecretsDictionary()
    {
        if (!File.Exists(_secretsFilePath))
        {
            return new Dictionary<string, string>();
        }

        try
        {
            var json = File.ReadAllText(_secretsFilePath);
            return JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
        }
        catch
        {
            // Trả về dict trống nếu file hỏng
            return new Dictionary<string, string>();
        }
    }

    private void SaveSecretsDictionary(Dictionary<string, string> dictionary)
    {
        var json = JsonSerializer.Serialize(dictionary, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_secretsFilePath, json);
    }
}
