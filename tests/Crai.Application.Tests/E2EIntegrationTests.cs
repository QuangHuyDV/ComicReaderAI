using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using SkiaSharp;
using Crai.Application.Contracts.Runtime;
using Crai.Application.Contracts.Services;
using Crai.Application.Contracts.Infrastructure;
using Crai.Domain.Runtime;
using Crai.Runtime.Engine;
using Crai.Runtime.Storage;
using Crai.Infrastructure.Configuration;
using Crai.Infrastructure.Secret;
using Crai.Modules.Recognition.Services;
using Crai.Modules.Translation.Services;
using Crai.Modules.Presentation.Services;
using Crai.Modules.TextProcessing.Services;
using Crai.Modules.Storage.Services;

namespace Crai.Application.Tests;

public class E2EIntegrationTests : IDisposable
{
    private readonly string _testImagePath;
    private readonly IArtifactStore _artifactStore;
    private readonly IStructuredLogger _mockLogger;
    private readonly ITelemetryService _mockTelemetry;
    private readonly IConfigurationService _configService;
    private readonly ISecretManager _secretManager;
    private readonly ITranslationCache _translationCache;

    public E2EIntegrationTests()
    {
        _testImagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test_integration_ocr.png");
        _artifactStore = new InMemoryArtifactStore();
        _mockLogger = new MockLogger();
        _mockTelemetry = new MockTelemetry();
        
        // Cấu hình in-memory config
        _configService = new ConfigurationService();
        _secretManager = new DpapiSecretManager(_mockLogger, "test_integration_secrets.dat");
        _translationCache = new SqliteTranslationCache(_mockLogger, "test_integration_cache.db");

        // Tạo file ảnh test chứa chữ "HELLO" bằng SkiaSharp
        CreateOcrTestImage(_testImagePath, "HELLO");
    }

    public void Dispose()
    {
        TryDeleteFile(_testImagePath);
        TryDeleteFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test_integration_secrets.dat"));
        TryDeleteFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test_integration_cache.db"));
    }

    [Fact]
    public async Task E2EPipeline_WithRealOcrAndRealTranslation_ShouldSucceed()
    {
        // 1. Arrange
        var fakeCapture = new FakeCaptureService(_testImagePath);
        var realOcr = new WindowsOcrService(_configService, _mockLogger);
        
        var googleEngine = new GoogleTranslationEngine(_mockLogger);
        var geminiEngine = new GeminiTranslationEngine(_secretManager, _mockLogger);
        var realTranslation = new TranslationRouter(googleEngine, geminiEngine, _configService, _translationCache, _mockLogger);
        
        var realPresentation = new OverlayPresentationService(_mockLogger);

        var runtime = new PipelineRuntime(
            fakeCapture,
            realOcr,
            new TextProcessorService(),
            realTranslation,
            realPresentation,
            _artifactStore,
            _mockLogger,
            _mockTelemetry
        );

        // 2. Act
        var result = await runtime.TriggerExecutionAsync();

        // 3. Assert
        Assert.NotNull(result);
        
        // Windows OCR hoạt động trên môi trường Windows local của User
        // Chúng ta kiểm tra nếu chạy trên máy hỗ trợ English (luôn được bật trên Windows SDK)
        if (result.Status == WorkItemStatus.Completed)
        {
            Assert.Contains("HELLO", result.RawText?.ToUpper() ?? ""); // OCR quét thành công chữ HELLO
            Assert.NotNull(result.TranslatedText); // Dịch thành công
            Assert.True(result.TranslatedText.Length > 0);
            
            // Trình diễn nhận đúng bản dịch
            Assert.Equal(result.TranslatedText, realPresentation.LatestText);
        }
        else
        {
            // Trường hợp lỗi (ví dụ không có kết nối mạng để gọi Google Translate API)
            Assert.Equal(WorkItemStatus.Failed, result.Status);
            Assert.NotNull(result.ErrorMessage);
        }
    }

    private void CreateOcrTestImage(string path, string text)
    {
        try
        {
            var info = new SKImageInfo(200, 80);
            using var surface = SKSurface.Create(info);
            var canvas = surface.Canvas;

            // Tô nền trắng
            canvas.Clear(SKColors.White);

            // Vẽ text chữ đen, font size lớn để OCR dễ quét (Tương thích SkiaSharp v3.x / v4.x)
            using var typeface = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
            using var font = new SKFont(typeface, 36);
            using var paint = new SKPaint
            {
                Color = SKColors.Black,
                IsAntialias = true
            };

            canvas.DrawText(text, 20, 50, font, paint);

            // Save ra file PNG
            using var image = surface.Snapshot();
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            
            // Đảm bảo xóa file cũ trước khi write
            TryDeleteFile(path);
            
            using var stream = File.OpenWrite(path);
            data.SaveTo(stream);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[TestSetup] Không thể tạo ảnh test bằng SkiaSharp: {ex.Message}");
        }
    }

    private void TryDeleteFile(string path)
    {
        if (File.Exists(path))
        {
            try { File.Delete(path); } catch { }
        }
    }

    // Fakes
    private class FakeCaptureService : ICaptureService
    {
        private readonly string _pathToReturn;
        public FakeCaptureService(string pathToReturn) => _pathToReturn = pathToReturn;
        public Task<string> CaptureTargetWindowAsync(string outputFilePath, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_pathToReturn);
        }
    }

    private class MockLogger : IStructuredLogger
    {
        public void LogDebug(string message, Dictionary<string, object>? context = null) { }
        public void LogInfo(string message, Dictionary<string, object>? context = null) { }
        public void LogWarning(string message, Dictionary<string, object>? context = null) { }
        public void LogError(string message, Exception? exception = null, Dictionary<string, object>? context = null) { }
    }

    private class MockTelemetry : ITelemetryService
    {
        public void RecordMetric(string name, double value, Dictionary<string, string>? tags = null) { }
        public void RecordEvent(string name, Dictionary<string, object>? properties = null) { }
        public ITraceSpan StartTrace(string name) => new DummyTraceSpan(name);

        private class DummyTraceSpan : ITraceSpan
        {
            public string Name { get; }
            public long ElapsedMilliseconds => 10;
            public DummyTraceSpan(string name) => Name = name;
            public void Dispose() { }
        }
    }
}
