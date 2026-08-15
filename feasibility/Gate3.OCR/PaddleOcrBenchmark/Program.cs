using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenCvSharp;
using Sdcb.PaddleOCR;
using Sdcb.PaddleInference;
using Sdcb.PaddleOCR.Models.Local;
using SkiaSharp;
using Windows.Media.Ocr;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;

namespace PaddleOcrBenchmark;

class Program
{
    private static readonly string DatasetDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dataset");
    private static readonly string OutputDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "output");

    static async Task Main(string[] args)
    {
        Console.WriteLine("====================================================");
        Console.WriteLine("CRAI Gate 3 - OCR Feasibility & Benchmark Comparison");
        Console.WriteLine("====================================================");

        try
        {
            // 1. Tạo dataset test tự sinh (được chia dòng để tránh tràn ảnh)
            PrepareSyntheticDataset();

            // 2. Chạy benchmark PaddleOCR
            RunPaddleOcrBenchmark();

            // 3. Chạy benchmark Windows Media OCR
            await RunWindowsOcrBenchmarkAsync();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n[FATAL ERROR] {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            Console.ResetColor();
        }
    }

    private static void PrepareSyntheticDataset()
    {
        Console.WriteLine("\n[1/3] Chuẩn bị dữ liệu thử nghiệm (Synthetic Dataset)...");
        if (Directory.Exists(DatasetDir)) Directory.Delete(DatasetDir, true);
        Directory.CreateDirectory(DatasetDir);

        // Tạo các mẫu text tiếng Trung đại diện cho các trường hợp manhua/novel
        // Đặt dấu xuống dòng \n thủ công để ảnh không bị tràn và mất chữ
        var testSamples = new List<(string Name, string Text, bool Vertical, SKColor BgColor, SKColor TextColor)>
        {
            (
                "sample_01_horizontal_novel",
                "第一章 重生之日\n那是雷鸣交加的雨夜，\n天空仿佛被撕裂开来。\n林默站在天台上，\n冷冷地看着下方的城市。",
                false,
                SKColors.White,
                SKColors.Black
            ),
            (
                "sample_02_vertical_manhua",
                "放手！\n你这恶徒\n竟敢伤我！",
                true,
                SKColors.White,
                SKColors.Black
            ),
            (
                "sample_03_bubble_stylized",
                "轰隆隆！\n天帝神拳！",
                false,
                new SKColor(255, 230, 230), // Nền hồng nhạt giả lập bubble
                SKColors.Red // Chữ đỏ cường điệu
            ),
            (
                "sample_04_traditional_chinese",
                "第一章 重生之日\n那是雷鳴交加的雨夜，\n天空彷彿被撕裂開來。\n林默站在天台上，\n冷冷地看著下方的城市。",
                false,
                SKColors.White,
                SKColors.DarkBlue
            ),
            (
                "sample_05_mixed_english",
                "CRAI System status: 运行正常。\nLatency is less than 50ms.\n性能优异！",
                false,
                new SKColor(240, 240, 240),
                SKColors.DarkSlateGray
            )
        };

        foreach (var sample in testSamples)
        {
            CreateTextImage(sample.Name, sample.Text, sample.Vertical, sample.BgColor, sample.TextColor);
        }
        Console.WriteLine($"✓ Đã sinh thành công {testSamples.Count} ảnh test trong thư mục: {DatasetDir}");
    }

    private static void CreateTextImage(string name, string text, bool vertical, SKColor bgColor, SKColor textColor)
    {
        int width = 800;
        int height = 500; // Tăng chiều cao để đủ chỗ cho chữ xuống dòng

        using var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(bgColor);

        var fontName = "Microsoft YaHei";
        using var typeface = SKTypeface.FromFamilyName(fontName, SKFontStyleWeight.Normal, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
        using var font = new SKFont(typeface, 28);
        
        using var paint = new SKPaint();
        paint.Color = textColor;
        paint.IsAntialias = true;

        string[] lines = text.Split('\n');
        float y = 50;

        if (vertical)
        {
            float x = width - 80;
            foreach (var line in lines)
            {
                float charY = 50;
                foreach (char c in line)
                {
                    canvas.DrawText(c.ToString(), x, charY, font, paint);
                    charY += font.Size + 8;
                }
                x -= 50;
            }
        }
        else
        {
            foreach (var line in lines)
            {
                canvas.DrawText(line, 40, y, font, paint);
                y += font.Size + 15;
            }
        }

        string imgPath = Path.Combine(DatasetDir, name + ".png");
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = File.OpenWrite(imgPath);
        data.SaveTo(stream);

        string txtPath = Path.Combine(DatasetDir, name + ".txt");
        File.WriteAllText(txtPath, text, Encoding.UTF8);
    }

    private static void RunPaddleOcrBenchmark()
    {
        Console.WriteLine("\n[2/3] Chạy PaddleOCR (Local Chinese V5) Benchmark...");
        
        if (Directory.Exists(OutputDir)) Directory.Delete(OutputDir, true);
        Directory.CreateDirectory(OutputDir);

        var swInit = Stopwatch.StartNew();
        using var ocr = new PaddleOcrAll(LocalFullModels.ChineseV5, PaddleDevice.Openblas());
        swInit.Stop();
        Console.WriteLine($"✓ Khởi tạo PaddleOCR Engine trong: {swInit.ElapsedMilliseconds} ms");

        string[] imgFiles = Directory.GetFiles(DatasetDir, "*.png");
        double totalLatency = 0;
        double totalCer = 0;
        int count = 0;

        Console.WriteLine("\n--- PADDLEOCR CHI TIẾT ---");

        foreach (var imgFile in imgFiles)
            {
            string name = Path.GetFileNameWithoutExtension(imgFile);
            string gtText = File.ReadAllText(Path.Combine(DatasetDir, name + ".txt")).Replace("\n", " ").Replace("\r", "").Trim();

            using var mat = Cv2.ImRead(imgFile);
            
            var swInference = Stopwatch.StartNew();
            PaddleOcrResult ocrResult = ocr.Run(mat);
            swInference.Stop();

            string ocrText = ocrResult.Text.Replace("\n", " ").Replace("\r", "").Trim();
            double cer = CalculateCer(gtText, ocrText);
            
            totalLatency += swInference.ElapsedMilliseconds;
            totalCer += cer;
            count++;

            Console.WriteLine($"[Mẫu: {name}]");
            Console.WriteLine($"  - Latency: {swInference.ElapsedMilliseconds} ms");
            Console.WriteLine($"  - Accuracy: {((1 - cer) * 100):F2}% (CER: {(cer * 100):F2}%)");
            Console.WriteLine($"  - Output  : \"{ocrText}\"");

            // Lưu hình ảnh phát hiện khung chữ
            using var matDraw = mat.Clone();
            foreach (var region in ocrResult.Regions)
            {
                var pts = region.Rect.Points();
                for (int i = 0; i < 4; i++)
                {
                    Cv2.Line(matDraw, 
                        new Point((int)pts[i].X, (int)pts[i].Y), 
                        new Point((int)pts[(i + 1) % 4].X, (int)pts[(i + 1) % 4].Y), 
                        Scalar.Red, 2);
                }
            }
            Cv2.ImWrite(Path.Combine(OutputDir, $"paddle_{name}_detected.png"), matDraw);
        }

        double avgLatency = totalLatency / count;
        double avgCer = totalCer / count;

        Console.WriteLine("\n--> PADDLEOCR KẾT QUẢ TRUNG BÌNH:");
        Console.WriteLine($"    - Latency: {avgLatency:F2} ms / ảnh");
        Console.WriteLine($"    - Accuracy: {((1 - avgCer) * 100):F2}%");
    }

    private static async Task RunWindowsOcrBenchmarkAsync()
    {
        Console.WriteLine("\n[3/3] Chạy Windows built-in Media OCR Benchmark...");

        // 1. Kiểm tra các ngôn ngữ được Windows OCR hỗ trợ
        var supportedLanguages = OcrEngine.AvailableRecognizerLanguages;
        Console.WriteLine("Các ngôn ngữ OCR được cài đặt trên hệ thống Windows này:");
        bool hasChinese = false;
        foreach (var lang in supportedLanguages)
        {
            Console.WriteLine($"  - {lang.LanguageTag} ({lang.DisplayName})");
            if (lang.LanguageTag.StartsWith("zh", StringComparison.OrdinalIgnoreCase))
            {
                hasChinese = true;
            }
        }

        string targetLang = hasChinese ? "zh-Hans" : "en-US";
        Console.WriteLine($"-> Sử dụng ngôn ngữ Windows OCR: '{targetLang}'");

        var swInit = Stopwatch.StartNew();
        var language = new Windows.Globalization.Language(targetLang);
        var ocrEngine = OcrEngine.TryCreateFromLanguage(language);
        swInit.Stop();

        if (ocrEngine == null)
        {
            Console.WriteLine("⚠️ Không thể khởi tạo Windows OcrEngine!");
            return;
        }
        Console.WriteLine($"✓ Khởi tạo Windows OcrEngine trong: {swInit.ElapsedMilliseconds} ms");

        string[] imgFiles = Directory.GetFiles(DatasetDir, "*.png");
        double totalLatency = 0;
        double totalCer = 0;
        int count = 0;

        Console.WriteLine("\n--- WINDOWS MEDIA OCR CHI TIẾT ---");

        foreach (var imgFile in imgFiles)
        {
            string name = Path.GetFileNameWithoutExtension(imgFile);
            string gtText = File.ReadAllText(Path.Combine(DatasetDir, name + ".txt")).Replace("\n", " ").Replace("\r", "").Trim();

            var swInference = Stopwatch.StartNew();
            
            // Đọc file ảnh sang SoftwareBitmap của WinRT
            var storageFile = await Windows.Storage.StorageFile.GetFileFromPathAsync(imgFile);
            using var stream = await storageFile.OpenAsync(Windows.Storage.FileAccessMode.Read);
            var decoder = await BitmapDecoder.CreateAsync(stream);
            using var softwareBitmap = await decoder.GetSoftwareBitmapAsync();

            // Nhận diện
            var ocrResult = await ocrEngine.RecognizeAsync(softwareBitmap);
            swInference.Stop();

            string ocrText = ocrResult.Text.Replace("\n", " ").Replace("\r", "").Trim();
            double cer = CalculateCer(gtText, ocrText);

            totalLatency += swInference.ElapsedMilliseconds;
            totalCer += cer;
            count++;

            Console.WriteLine($"[Mẫu: {name}]");
            Console.WriteLine($"  - Latency: {swInference.ElapsedMilliseconds} ms");
            Console.WriteLine($"  - Accuracy: {((1 - cer) * 100):F2}% (CER: {(cer * 100):F2}%)");
            Console.WriteLine($"  - Output  : \"{ocrText}\"");
        }

        double avgLatency = totalLatency / count;
        double avgCer = totalCer / count;

        Console.WriteLine("\n--> WINDOWS MEDIA OCR KẾT QUẢ TRUNG BÌNH:");
        Console.WriteLine($"    - Latency: {avgLatency:F2} ms / ảnh");
        Console.WriteLine($"    - Accuracy: {((1 - avgCer) * 100):F2}%");
    }

    private static double CalculateCer(string groundTruth, string prediction)
    {
        // Loại bỏ khoảng trắng khi so sánh tiếng Trung để giảm nhiễu (PaddleOCR và WinRT OCR có cách chèn khoảng trắng khác nhau)
        string gtClean = groundTruth.Replace(" ", "").Replace("\t", "");
        string predClean = prediction.Replace(" ", "").Replace("\t", "");

        if (string.IsNullOrEmpty(gtClean))
            return string.IsNullOrEmpty(predClean) ? 0 : 1;
        if (string.IsNullOrEmpty(predClean))
            return 1;

        int[,] d = new int[gtClean.Length + 1, predClean.Length + 1];

        for (int i = 0; i <= gtClean.Length; i++) d[i, 0] = i;
        for (int j = 0; j <= predClean.Length; j++) d[0, j] = j;

        for (int i = 1; i <= gtClean.Length; i++)
        {
            for (int j = 1; j <= predClean.Length; j++)
            {
                int cost = (predClean[j - 1] == gtClean[i - 1]) ? 0 : 1;
                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost
                );
            }
        }

        int distance = d[gtClean.Length, predClean.Length];
        return (double)distance / gtClean.Length;
    }
}
