using System;
using System.Text;
using System.Text.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Crai.Application.Contracts.Infrastructure;

namespace Crai.Modules.Translation.Services;

public class GeminiTranslationEngine
{
    private static readonly HttpClient HttpClient = new HttpClient();
    private readonly ISecretManager _secretManager;
    private readonly IStructuredLogger _logger;

    public GeminiTranslationEngine(ISecretManager secretManager, IStructuredLogger logger)
    {
        _secretManager = secretManager ?? throw new ArgumentNullException(nameof(secretManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> TranslateAsync(string text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;

        // 1. Lấy API Key an toàn từ Windows DPAPI Secret Manager
        var apiKey = _secretManager.GetSecret("GeminiApiKey");
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogWarning("[GeminiTranslationEngine] Chưa cấu hình GeminiApiKey trong Secret Manager. Yêu cầu dịch sẽ bị bỏ qua hoặc chuyển sang fallback.");
            throw new InvalidOperationException("Gemini API Key is not configured.");
        }

        try
        {
            _logger.LogDebug("[GeminiTranslationEngine] Đang gửi yêu cầu dịch thuật ngữ tới Gemini 1.5 Flash API...");

            var endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={apiKey}";

            // Định nghĩa System Instruction chứa Glossary thuật ngữ Tu Tiên / Xianxia chuyên biệt để sửa lỗi dịch sai
            var systemInstruction = "Bạn là dịch giả truyện tranh chuyên nghiệp Trung-Việt và Anh-Việt. Hãy dịch đoạn văn bản sau sang tiếng Việt một cách tự nhiên, mượt mà và phù hợp ngữ cảnh truyện tranh. " +
                                    "Đặc biệt chú ý sử dụng chính xác các thuật ngữ Tu Tiên (Xianxia/Manhua) sau đây:\n" +
                                    "- \"Foundation Establishment\" hoặc \"Zhu Ji\" hoặc \"筑基\" -> \"Trúc Cơ kỳ\"\n" +
                                    "- \"Golden Core\" hoặc \"Jin Dan\" hoặc \"金丹\" -> \"Kim Đan kỳ\"\n" +
                                    "- \"Nascent Soul\" hoặc \"Yuan Ying\" hoặc \"元婴\" -> \"Nguyên Anh kỳ\"\n" +
                                    "- \"Qi Condensation\" hoặc \"Lian Qi\" hoặc \"炼气\" -> \"Luyện Khí kỳ\"\n" +
                                    "Chỉ trả về chuỗi kết quả dịch duy nhất, không thêm lời giải thích nào khác.";

            // Tạo cấu trúc request body cho Gemini API
            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = text }
                        }
                    }
                },
                systemInstruction = new
                {
                    parts = new[]
                    {
                        new { text = systemInstruction }
                    }
                },
                safetySettings = new[]
                {
                    new { category = "HARM_CATEGORY_HARASSMENT", threshold = "BLOCK_NONE" },
                    new { category = "HARM_CATEGORY_HATE_SPEECH", threshold = "BLOCK_NONE" },
                    new { category = "HARM_CATEGORY_SEXUALLY_EXPLICIT", threshold = "BLOCK_NONE" },
                    new { category = "HARM_CATEGORY_DANGEROUS_CONTENT", threshold = "BLOCK_NONE" }
                }
            };

            var jsonPayload = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var response = await HttpClient.PostAsync(endpoint, content, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new HttpRequestException($"Gemini API error code {response.StatusCode}: {errorContent}");
            }

            var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(responseString);
            
            // Parse kết quả trả về từ Gemini JSON structure: candidates[0].content.parts[0].text
            var candidates = doc.RootElement.GetProperty("candidates");
            var firstCandidate = candidates[0];
            var parts = firstCandidate.GetProperty("content").GetProperty("parts");
            var translatedResult = parts[0].GetProperty("text").GetString() ?? string.Empty;

            var finalResult = translatedResult.Trim();
            _logger.LogDebug("[GeminiTranslationEngine] Dịch thành công bằng Gemini 1.5 Flash API (đã tiêm Glossary).");
            return finalResult;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError($"[GeminiTranslationEngine] Lỗi dịch thuật Gemini: {ex.Message}", ex);
            throw new InvalidOperationException($"Lỗi dịch Gemini: {ex.Message}", ex);
        }
    }
}
