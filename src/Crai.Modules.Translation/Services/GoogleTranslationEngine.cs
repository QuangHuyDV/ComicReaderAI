using System;
using System.Text;
using System.Text.Json;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Crai.Application.Contracts.Infrastructure;

namespace Crai.Modules.Translation.Services;

public class GoogleTranslationEngine
{
    private static readonly HttpClient HttpClient = new HttpClient();
    private readonly IStructuredLogger _logger;

    public GoogleTranslationEngine(IStructuredLogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> TranslateAsync(string text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;

        try
        {
            _logger.LogDebug("[GoogleTranslationEngine] Đang gửi yêu cầu dịch tới Google Translate Web API...");

            // URL dịch thuật miễn phí của Google
            string url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl=auto&tl=vi&dt=t&q={Uri.EscapeDataString(text)}";
            
            var response = await HttpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(responseString);
            
            var arr = doc.RootElement[0];
            var sb = new StringBuilder();

            foreach (var item in arr.EnumerateArray())
            {
                sb.Append(item[0].GetString());
            }

            var result = sb.ToString().Trim();
            _logger.LogDebug("[GoogleTranslationEngine] Dịch thành công bằng Google Translate Web API.");
            return result;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError($"[GoogleTranslationEngine] Lỗi dịch thuật: {ex.Message}", ex);
            throw new InvalidOperationException($"Lỗi dịch Google: {ex.Message}", ex);
        }
    }
}
