using System;
using System.Threading;
using System.Threading.Tasks;
using Crai.Application.Contracts.Services;
using Crai.Application.Contracts.Infrastructure;

namespace Crai.Modules.Translation.Services;

public class TranslationRouter : ITranslationService
{
    private readonly GoogleTranslationEngine _googleEngine;
    private readonly GeminiTranslationEngine _geminiEngine;
    private readonly IConfigurationService _configService;
    private readonly ITranslationCache _translationCache;
    private readonly IStructuredLogger _logger;

    /// <summary>
    /// Cho phép ghi đè cấu hình Engine dịch động tại runtime.
    /// </summary>
    public static string? PreferredEngineOverride { get; set; }

    public TranslationRouter(
        GoogleTranslationEngine googleEngine,
        GeminiTranslationEngine geminiEngine,
        IConfigurationService configService,
        ITranslationCache translationCache,
        IStructuredLogger logger)
    {
        _googleEngine = googleEngine ?? throw new ArgumentNullException(nameof(googleEngine));
        _geminiEngine = geminiEngine ?? throw new ArgumentNullException(nameof(geminiEngine));
        _configService = configService ?? throw new ArgumentNullException(nameof(configService));
        _translationCache = translationCache ?? throw new ArgumentNullException(nameof(translationCache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> TranslateTextAsync(string rawText, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(rawText))
        {
            return string.Empty;
        }

        var targetLanguage = "vi"; // Dịch mặc định sang Tiếng Việt

        // 1. Kiểm tra Cache cục bộ trước khi gọi Engine (Tối ưu hóa tốc độ và chi phí)
        try
        {
            var cachedResult = await _translationCache.GetAsync(rawText, targetLanguage, cancellationToken);
            if (!string.IsNullOrWhiteSpace(cachedResult))
            {
                _logger.LogInfo("[TranslationRouter] Cache HIT! Trả về kết quả dịch từ SQLite Database cục bộ.");
                return cachedResult;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"[TranslationRouter] Không thể truy cập Cache dịch thuật: {ex.Message}");
        }

        // Lấy engine mong muốn từ cấu hình hệ thống hoặc ghi đè runtime
        var preferredEngine = PreferredEngineOverride ?? _configService.GetValue<string>("Translation:Engine") ?? "GoogleTranslate";
        string translatedText;

        if (preferredEngine.Equals("Gemini", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                // Thử dịch bằng Gemini Engine
                translatedText = await _geminiEngine.TranslateAsync(rawText, cancellationToken);
            }
            catch (Exception ex)
            {
                // Cơ chế FALLBACK tự động sang Google Translate khi Gemini lỗi
                _logger.LogWarning($"[TranslationRouter] Tiến trình dịch bằng Gemini gặp lỗi ({ex.Message}). Tự động kích hoạt cơ chế Fallback sang Google Translate Web API...");
                
                translatedText = await _googleEngine.TranslateAsync(rawText, cancellationToken);
            }
        }
        else
        {
            // Mặc định hoặc cấu hình rõ là GoogleTranslate
            translatedText = await _googleEngine.TranslateAsync(rawText, cancellationToken);
        }

        // 2. Lưu kết quả dịch mới vào Cache cục bộ
        if (!string.IsNullOrWhiteSpace(translatedText))
        {
            try
            {
                await _translationCache.SetAsync(rawText, targetLanguage, translatedText, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"[TranslationRouter] Không thể lưu kết quả dịch vào Cache SQLite: {ex.Message}");
            }
        }

        return translatedText;
    }
}
