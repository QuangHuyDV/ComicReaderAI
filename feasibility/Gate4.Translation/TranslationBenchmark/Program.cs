using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace TranslationBenchmark;

class Program
{
    private static readonly HttpClient client = new HttpClient();

    static async Task Main(string[] args)
    {
        Console.WriteLine("====================================================");
        Console.WriteLine("CRAI Gate 4 - Translation Feasibility Benchmark");
        Console.WriteLine("====================================================");

        // 1. Định nghĩa bộ dataset kiểm thử (Fiction & Manhua)
        var dataset = GetTranslationDataset();

        // 2. Chạy Google Translate (Free web API)
        Console.WriteLine("\n[1/2] Đang chạy Google Translate API benchmark...");
        var googleResults = await RunGoogleTranslateBenchmark(dataset);

        // 3. Chạy Gemini API (nếu có API Key trong environment)
        Console.WriteLine("\n[2/2] Đang chạy Gemini 1.5 Flash API benchmark...");
        var geminiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
        List<TranslationResult>? geminiResults = null;

        if (string.IsNullOrEmpty(geminiKey))
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("⚠️ Bỏ qua Gemini benchmark do thiếu biến môi trường 'GEMINI_API_KEY'.");
            Console.WriteLine("💡 Để chạy Gemini, hãy set environment variable: $env:GEMINI_API_KEY='your-key'");
            Console.ResetColor();
        }
        else
        {
            geminiResults = await RunGeminiBenchmark(dataset, geminiKey);
        }

        // 4. In báo cáo tổng hợp
        PrintSummaryReport(dataset, googleResults, geminiResults);
    }

    private static List<TranslationSample> GetTranslationDataset()
    {
        return new List<TranslationSample>
        {
            new TranslationSample(
                "xianxia_terms",
                "zh-CN",
                "那名炼气期修士祭出飞剑，试图抵挡筑基期大能的全力一击。",
                "Tu sĩ Luyện Khí kỳ đó tế ra phi kiếm, ý đồ chống đỡ toàn lực một kích của đại năng Trúc Cơ kỳ."
            ),
            new TranslationSample(
                "pronoun_addressing",
                "zh-CN",
                "陛下，本王觉得此事必有蹊跷，还请陛下三思。",
                "Bệ hạ, bản vương cảm thấy việc này tất có ẩn tình, còn xin bệ hạ tam tư."
            ),
            new TranslationSample(
                "comic_fragment",
                "zh-CN",
                "放手！你这恶徒，竟敢伤我师妹！",
                "Buông tay! Ngươi là tên ác đồ này, dám làm bị thương sư muội ta!"
            ),
            new TranslationSample(
                "traditional_chinese",
                "zh-TW",
                "第一章 重生之日。那是雷鳴交加的雨夜，天空彷彿被撕裂開來。",
                "Chương một: Ngày trùng sinh. Đó là đêm mưa sấm chớp đan xen, bầu trời phảng phất như bị xé rách."
            ),
            new TranslationSample(
                "mixed_slang",
                "zh-CN",
                "太给力了！这波操作简直是yyds！",
                "Quá tuyệt vời! Pha xử lý này giản trực là mãi mãi thần thánh (yyds)!"
            )
        };
    }

    private static async Task<List<TranslationResult>> RunGoogleTranslateBenchmark(List<TranslationSample> dataset)
    {
        var results = new List<TranslationResult>();
        foreach (var sample in dataset)
        {
            var sw = Stopwatch.StartNew();
            string translated = "";
            bool success = false;
            try
            {
                // Gọi Google Translate free web endpoint
                string url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl=auto&tl=vi&dt=t&q={Uri.EscapeDataString(sample.SourceText)}";
                var responseString = await client.GetStringAsync(url);
                
                // Parse JSON array kết quả: [[["Dịch...", "Nguồn...", ...]]]
                using var doc = JsonDocument.Parse(responseString);
                var arr = doc.RootElement[0];
                var sb = new StringBuilder();
                foreach (var item in arr.EnumerateArray())
                {
                    sb.Append(item[0].GetString());
                }
                translated = sb.ToString().Trim();
                success = true;
            }
            catch (Exception ex)
            {
                translated = $"[ERROR] {ex.Message}";
            }
            sw.Stop();

            results.Add(new TranslationResult(sample.Id, translated, sw.ElapsedMilliseconds, success));
            Console.WriteLine($"  - [{sample.Id}]: {sw.ElapsedMilliseconds} ms | Status: {(success ? "OK" : "FAIL")}");
        }
        return results;
    }

    private static async Task<List<TranslationResult>> RunGeminiBenchmark(List<TranslationSample> dataset, string apiKey)
    {
        var results = new List<TranslationResult>();
        string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={apiKey}";

        foreach (var sample in dataset)
        {
            var sw = Stopwatch.StartNew();
            string translated = "";
            bool success = false;
            try
            {
                // Xây dựng system prompt hướng dẫn dịch thuật văn học/manhua Trung-Việt
                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new { text = $"You are an expert translator specializing in translating Chinese novels and manhua (comics) into Vietnamese. Translate the following Chinese text into natural, literary Vietnamese, keeping names and terminology consistent with Sino-Vietnamese (Hán-Việt) style. Output ONLY the translated text, without explanation.\n\nText: {sample.SourceText}" }
                            }
                        }
                    }
                };

                string jsonPayload = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                
                var response = await client.PostAsync(url, content);
                if (response.IsSuccessStatusCode)
                {
                    string responseString = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(responseString);
                    translated = doc.RootElement
                        .GetProperty("candidates")[0]
                        .GetProperty("content")
                        .GetProperty("parts")[0]
                        .GetProperty("text")
                        .GetString() ?? "";
                    translated = translated.Trim();
                    success = true;
                }
                else
                {
                    translated = $"[ERROR API Status: {response.StatusCode}]";
                }
            }
            catch (Exception ex)
            {
                translated = $"[ERROR] {ex.Message}";
            }
            sw.Stop();

            results.Add(new TranslationResult(sample.Id, translated, sw.ElapsedMilliseconds, success));
            Console.WriteLine($"  - [{sample.Id}]: {sw.ElapsedMilliseconds} ms | Status: {(success ? "OK" : "FAIL")}");
        }
        return results;
    }

    private static void PrintSummaryReport(
        List<TranslationSample> dataset, 
        List<TranslationResult> googleResults, 
        List<TranslationResult>? geminiResults)
    {
        Console.WriteLine("\n====================================================");
        Console.WriteLine("BÁO CÁO CHI TIẾT TRANSLATION BENCHMARK");
        Console.WriteLine("====================================================");

        double totalGoogleLatency = 0;
        double totalGeminiLatency = 0;
        int count = dataset.Count;

        for (int i = 0; i < count; i++)
        {
            var sample = dataset[i];
            var google = googleResults[i];
            var gemini = geminiResults != null ? geminiResults[i] : null;

            totalGoogleLatency += google.LatencyMs;
            if (gemini != null) totalGeminiLatency += gemini.LatencyMs;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n[Mẫu {i + 1}: {sample.Id}]");
            Console.ResetColor();
            Console.WriteLine($"  Source: \"{sample.SourceText}\"");
            Console.WriteLine($"  Reference (Hán-Việt): \"{sample.ExpectedTranslation}\"");
            Console.WriteLine($"  Google Translate    : \"{google.OutputText}\" ({google.LatencyMs} ms)");
            if (gemini != null)
            {
                Console.WriteLine($"  Gemini 1.5 Flash    : \"{gemini.OutputText}\" ({gemini.LatencyMs} ms)");
            }
        }

        Console.WriteLine("\n====================================================");
        Console.WriteLine("TỔNG HỢP HIỆU NĂNG");
        Console.WriteLine("====================================================");
        Console.WriteLine($"- Latency trung bình Google Translate : {(totalGoogleLatency / count):F2} ms");
        if (geminiResults != null)
        {
            Console.WriteLine($"- Latency trung bình Gemini 1.5 Flash : {(totalGeminiLatency / count):F2} ms");
        }
        Console.WriteLine("====================================================");
    }
}

public record TranslationSample(string Id, string SourceLang, string SourceText, string ExpectedTranslation);
public record TranslationResult(string SampleId, string OutputText, long LatencyMs, bool Success);
