using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SkiaSharp;
using Crai.Application.Contracts.Services;
using Crai.Application.Contracts.Infrastructure;

namespace Crai.Modules.Recognition.Services;

public class AiOcrService : IRecognitionService
{
    private readonly IConfigurationService _configService;
    private readonly ISecretManager _secretManager;
    private readonly IStructuredLogger _logger;
    private readonly HttpClient _httpClient;

    // Cache to save tokens on static screens
    private static string? _lastImageHash;
    private static OcrResultInfo? _lastOcrResult;
    private static readonly object _cacheLock = new();

    public AiOcrService(IConfigurationService configService, ISecretManager secretManager, IStructuredLogger logger)
    {
        _configService = configService ?? throw new ArgumentNullException(nameof(configService));
        _secretManager = secretManager ?? throw new ArgumentNullException(nameof(secretManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpClient = new HttpClient();
    }

    public async Task<OcrResultInfo> RecognizeTextAsync(string imagePath, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
        {
            return new OcrResultInfo(string.Empty, new List<OcrLineInfo>());
        }

        try
        {
            // 1. Load image and compute hash to optimize token usage
            byte[] compressedBytes;
            int imgWidth, imgHeight;
            string currentHash;

            using (var codec = SKCodec.Create(imagePath))
            using (var bitmap = SKBitmap.Decode(codec))
            {
                currentHash = ComputeAverageHash(bitmap);

                // Compress image to save bandwidth and tokens
                int maxDim = 1024;
                int w = bitmap.Width;
                int h = bitmap.Height;
                if (w > maxDim || h > maxDim)
                {
                    if (w > h)
                    {
                        h = (int)(h * ((double)maxDim / w));
                        w = maxDim;
                    }
                    else
                    {
                        w = (int)(w * ((double)maxDim / h));
                        h = maxDim;
                    }
                }

                imgWidth = w;
                imgHeight = h;

                using (var resized = new SKBitmap(w, h))
                {
                    bitmap.ScalePixels(resized, new SKSamplingOptions(SKFilterMode.Linear));
                    using (var image = SKImage.FromBitmap(resized))
                    using (var data = image.Encode(SKEncodedImageFormat.Jpeg, 70))
                    {
                        compressedBytes = data.ToArray();
                    }
                }
            }

            // Check cache
            lock (_cacheLock)
            {
                if (_lastImageHash != null && _lastOcrResult != null)
                {
                    int distance = GetHammingDistance(currentHash, _lastImageHash);
                    if (distance <= 3) // Visually similar
                    {
                        _logger.LogInfo("[AiOcrService] Phat hien man hinh tinh (Khong thay doi). Su dung lai ban dich cu de tiet kiem Token.");
                        return _lastOcrResult;
                    }
                }
            }

            // 2. Perform AI OCR + Translation
            var provider = _configService.GetValue<string>("AI:Provider") ?? "Gemini";
            OcrResultInfo result;

            if (provider.Equals("OpenAI", StringComparison.OrdinalIgnoreCase))
            {
                result = await ExecuteOpenAiOcrAsync(compressedBytes, imgWidth, imgHeight, cancellationToken);
            }
            else
            {
                result = await ExecuteGeminiOcrAsync(compressedBytes, imgWidth, imgHeight, cancellationToken);
            }

            // Save to cache
            lock (_cacheLock)
            {
                _lastImageHash = currentHash;
                _lastOcrResult = result;
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"[AiOcrService] Loi dich quet bang AI: {ex.Message}", ex);
            throw;
        }
    }

    private async Task<OcrResultInfo> ExecuteGeminiOcrAsync(byte[] imageBytes, int w, int h, CancellationToken cancellationToken)
    {
        var apiKey = _secretManager.GetSecret("GeminiApiKey");
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("Gemini API Key chua duoc cau hinh.");
        }

        var endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={apiKey}";
        var base64Image = Convert.ToBase64String(imageBytes);

        var prompt = GetOcrPrompt(w, h);

        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new object[]
                    {
                        new { text = prompt },
                        new { inlineData = new { mimeType = "image/jpeg", data = base64Image } }
                    }
                }
            },
            generationConfig = new
            {
                responseMimeType = "application/json"
            }
        };

        var jsonRequest = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

        _logger.LogDebug("[AiOcrService] Gui yeu cau OCR + Dich den Gemini Vision API...");
        var response = await _httpClient.PostAsync(endpoint, content, cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Gemini Vision API error: {response.StatusCode} - {responseContent}");
        }

        using var doc = JsonDocument.Parse(responseContent);
        var root = doc.RootElement;
        
        var textResponse = root.GetProperty("candidates")[0]
                               .GetProperty("content")
                               .GetProperty("parts")[0]
                               .GetProperty("text")
                               .GetString();

        if (string.IsNullOrWhiteSpace(textResponse))
        {
            return new OcrResultInfo(string.Empty, new List<OcrLineInfo>());
        }

        return ParseAiOcrJson(textResponse, w, h);
    }

    private async Task<OcrResultInfo> ExecuteOpenAiOcrAsync(byte[] imageBytes, int w, int h, CancellationToken cancellationToken)
    {
        var apiKey = _secretManager.GetSecret("OpenAiApiKey");
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("OpenAI API Key chua duoc cau hinh.");
        }

        var endpoint = _configService.GetValue<string>("OpenAI:Endpoint");
        if (string.IsNullOrWhiteSpace(endpoint)) endpoint = "https://api.openai.com/v1";
        
        var model = _configService.GetValue<string>("OpenAI:Model");
        if (string.IsNullOrWhiteSpace(model)) model = "gpt-4o-mini";

        var base64Image = Convert.ToBase64String(imageBytes);
        var prompt = GetOcrPrompt(w, h);

        var requestBody = new
        {
            model = model,
            response_format = new { type = "json_object" },
            messages = new[]
            {
                new
                {
                    role = "user",
                    content = new object[]
                    {
                        new { type = "text", text = prompt },
                        new { type = "image_url", image_url = new { url = $"data:image/jpeg;base64,{base64Image}" } }
                    }
                }
            }
        };

        var jsonRequest = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

        using var request = new HttpRequestMessage(HttpMethod.Post, $"{endpoint}/chat/completions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Content = content;

        _logger.LogDebug($"[AiOcrService] Gui yeu cau OCR + Dich den OpenAI Vision API ({model})...");
        var response = await _httpClient.SendAsync(request, cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"OpenAI Vision API error: {response.StatusCode} - {responseContent}");
        }

        using var doc = JsonDocument.Parse(responseContent);
        var root = doc.RootElement;
        
        var textResponse = root.GetProperty("choices")[0]
                               .GetProperty("message")
                               .GetProperty("content")
                               .GetString();

        if (string.IsNullOrWhiteSpace(textResponse))
        {
            return new OcrResultInfo(string.Empty, new List<OcrLineInfo>());
        }

        return ParseAiOcrJson(textResponse, w, h);
    }

    private string GetOcrPrompt(int width, int height)
    {
        return "You are a professional comic OCR and translation assistant.\n" +
               "Identify all text bubbles, dialog boxes, or text areas in this image.\n" +
               "For each detected text block, output its boundary box and translate it into natural Vietnamese suitable for the comic context.\n" +
               $"The input image size is {width}x{height} pixels.\n" +
               "Format the output strictly as a JSON object with a \"lines\" array containing elements with these properties:\n" +
               "- \"text\": The detected original text.\n" +
               "- \"translated_text\": The Vietnamese translation.\n" +
               "- \"x\": Bounding box X coordinate in pixels.\n" +
               "- \"y\": Bounding box Y coordinate in pixels.\n" +
               "- \"width\": Bounding box width in pixels.\n" +
               "- \"height\": Bounding box height in pixels.\n" +
               "Do not output markdown code blocks formatting. Return ONLY raw JSON.";
    }

    private OcrResultInfo ParseAiOcrJson(string jsonText, int imgW, int imgH)
    {
        try
        {
            using var doc = JsonDocument.Parse(jsonText);
            var root = doc.RootElement;

            if (!root.TryGetProperty("lines", out var linesElement))
            {
                return new OcrResultInfo(string.Empty, new List<OcrLineInfo>());
            }

            var linesList = new List<OcrLineInfo>();
            var sbFullText = new StringBuilder();

            foreach (var element in linesElement.EnumerateArray())
            {
                var text = element.GetProperty("text").GetString() ?? string.Empty;
                var translatedText = element.GetProperty("translated_text").GetString() ?? string.Empty;
                var x = element.GetProperty("x").GetDouble();
                var y = element.GetProperty("y").GetDouble();
                var w = element.GetProperty("width").GetDouble();
                var h = element.GetProperty("height").GetDouble();

                var line = new OcrLineInfo(text, x, y, w, h)
                {
                    TranslatedText = translatedText
                };
                linesList.Add(line);

                if (sbFullText.Length > 0) sbFullText.Append("\n");
                sbFullText.Append(text);
            }

            return new OcrResultInfo(sbFullText.ToString(), linesList);
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"[AiOcrService] Loi parse ket qua JSON cua AI: {ex.Message}. Noi dung tho: {jsonText}");
            return new OcrResultInfo(string.Empty, new List<OcrLineInfo>());
        }
    }

    private static string ComputeAverageHash(SKBitmap bitmap)
    {
        using var small = new SKBitmap(8, 8);
        bitmap.ScalePixels(small, new SKSamplingOptions(SKFilterMode.Nearest));
        
        long sum = 0;
        int[] pixels = new int[64];
        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                var color = small.GetPixel(x, y);
                int gray = (int)(0.299 * color.Red + 0.587 * color.Green + 0.114 * color.Blue);
                pixels[y * 8 + x] = gray;
                sum += gray;
            }
        }
        
        long avg = sum / 64;
        long hash = 0;
        for (int i = 0; i < 64; i++)
        {
            if (pixels[i] >= avg)
            {
                hash |= (1L << i);
            }
        }
        return hash.ToString("X16");
    }

    private static int GetHammingDistance(string hash1, string hash2)
    {
        if (hash1.Length != hash2.Length) return 999;
        
        ulong h1 = ulong.Parse(hash1, System.Globalization.NumberStyles.HexNumber);
        ulong h2 = ulong.Parse(hash2, System.Globalization.NumberStyles.HexNumber);
        
        ulong diff = h1 ^ h2;
        int count = 0;
        while (diff > 0)
        {
            if ((diff & 1) == 1) count++;
            diff >>= 1;
        }
        return count;
    }
}
