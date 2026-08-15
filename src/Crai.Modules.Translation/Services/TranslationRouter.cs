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
    private readonly IStructuredLogger _logger;

    public TranslationRouter(
        GoogleTranslationEngine googleEngine,
        GeminiTranslationEngine geminiEngine,
        IConfigurationService configService,
        IStructuredLogger logger)
    {
        _googleEngine = googleEngine ?? throw new ArgumentNullException(nameof(googleEngine));
        _geminiEngine = geminiEngine ?? throw new ArgumentNullException(nameof(geminiEngine));
        _configService = configService ?? throw new ArgumentNullException(nameof(configService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> TranslateTextAsync(string rawText, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(rawText))
        {
            return string.Empty;
        }

        // Lấy engine mong muốn từ cấu hình hệ thống
        var preferredEngine = _configService.GetValue<string>("Translation:Engine") ?? "GoogleTranslate";

        if (preferredEngine.Equals("Gemini", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                // Thử dịch bằng Gemini Engine
                var result = await _geminiEngine.TranslateAsync(rawText, cancellationToken);
                return result;
            }
            catch (Exception ex)
            {
                // Cơ chế FALLBACK tự động sang Google Translate khi Gemini lỗi
                _logger.LogWarning($"[TranslationRouter] Tiến trình dịch bằng Gemini gặp lỗi ({ex.Message}). Tự động kích hoạt cơ chế Fallback sang Google Translate Web API...");
                
                return await _googleEngine.TranslateAsync(rawText, cancellationToken);
            }
        }

        // Mặc định hoặc cấu hình rõ là GoogleTranslate
        return await _googleEngine.TranslateAsync(rawText, cancellationToken);
    }
}
