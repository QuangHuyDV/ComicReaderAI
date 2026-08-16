using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Crai.Application.Contracts.Services;
using Crai.Application.Contracts.Infrastructure;

namespace Crai.Modules.Storage.Services;

public class SqliteTranslationCache : ITranslationCache
{
    private readonly string _connectionString;
    private readonly IStructuredLogger _logger;
    private readonly object _lock = new();

    public SqliteTranslationCache(IStructuredLogger logger, string? dbFileName = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        var fileName = dbFileName ?? "crai_cache.db";
        var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
        _connectionString = $"Data Source={dbPath};";

        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        lock (_lock)
        {
            try
            {
                using var connection = new SqliteConnection(_connectionString);
                connection.Open();

                var createTableSql = @"
                    CREATE TABLE IF NOT EXISTS translation_cache (
                        source_text TEXT,
                        target_language TEXT,
                        translated_text TEXT,
                        created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                        PRIMARY KEY (source_text, target_language)
                    );";

                using var command = new SqliteCommand(createTableSql, connection);
                command.ExecuteNonQuery();
                _logger.LogDebug("[SqliteTranslationCache] Khởi tạo Database SQLite thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"[SqliteTranslationCache] Không thể khởi tạo Database: {ex.Message}", ex);
            }
        }
    }

    public async Task<string?> GetAsync(string sourceText, string targetLanguage, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sourceText) || string.IsNullOrWhiteSpace(targetLanguage))
        {
            return null;
        }

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            var selectSql = @"
                SELECT translated_text 
                FROM translation_cache 
                WHERE source_text = @sourceText AND target_language = @targetLanguage;";

            using var command = new SqliteCommand(selectSql, connection);
            command.Parameters.AddWithValue("@sourceText", sourceText.Trim());
            command.Parameters.AddWithValue("@targetLanguage", targetLanguage.Trim().ToLower());

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                var cachedVal = reader.GetString(0);
                _logger.LogDebug($"[SqliteTranslationCache] Cache HIT cho văn bản gốc: \"{sourceText.Substring(0, Math.Min(sourceText.Length, 15))}...\"");
                return cachedVal;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"[SqliteTranslationCache] Lỗi đọc cache SQLite: {ex.Message}");
        }

        return null;
    }

    public async Task SetAsync(string sourceText, string targetLanguage, string translatedText, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sourceText) || string.IsNullOrWhiteSpace(targetLanguage) || string.IsNullOrWhiteSpace(translatedText))
        {
            return;
        }

        // Chạy bất đồng bộ an toàn, quản lý tranh chấp ghi bằng semaphore hoặc lock đơn giản
        await Task.Run(() =>
        {
            lock (_lock)
            {
                try
                {
                    using var connection = new SqliteConnection(_connectionString);
                    connection.Open();

                    var insertSql = @"
                        INSERT OR REPLACE INTO translation_cache (source_text, target_language, translated_text) 
                        VALUES (@sourceText, @targetLanguage, @translatedText);";

                    using var command = new SqliteCommand(insertSql, connection);
                    command.Parameters.AddWithValue("@sourceText", sourceText.Trim());
                    command.Parameters.AddWithValue("@targetLanguage", targetLanguage.Trim().ToLower());
                    command.Parameters.AddWithValue("@translatedText", translatedText.Trim());

                    command.ExecuteNonQuery();
                    _logger.LogDebug($"[SqliteTranslationCache] Đã lưu bản dịch thành công vào Cache.");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"[SqliteTranslationCache] Lỗi lưu cache SQLite: {ex.Message}", ex);
                }
            }
        }, cancellationToken);
    }

    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        await Task.Run(() =>
        {
            lock (_lock)
            {
                try
                {
                    using var connection = new SqliteConnection(_connectionString);
                    connection.Open();

                    var deleteSql = "DELETE FROM translation_cache;";
                    using var command = new SqliteCommand(deleteSql, connection);
                    command.ExecuteNonQuery();
                    _logger.LogInfo("[SqliteTranslationCache] Đã xóa sạch toàn bộ cache dịch thuật cục bộ.");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"[SqliteTranslationCache] Lỗi xóa cache SQLite: {ex.Message}", ex);
                }
            }
        }, cancellationToken);
    }
}
