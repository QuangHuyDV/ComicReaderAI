using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Crai.Application.Contracts.Runtime;
using Crai.Application.Contracts.Services;
using Crai.Application.Contracts.Infrastructure;
using Crai.Domain.Runtime;
using Crai.Runtime.Engine;
using Crai.Runtime.Storage;
using Crai.Modules.TextProcessing.Services;

namespace Crai.Application.Tests;

public class PipelineRuntimeTests
{
    private readonly IArtifactStore _artifactStore;
    private readonly MockLogger _mockLogger;
    private readonly MockTelemetry _mockTelemetry;
    private readonly ITextProcessorService _textProcessorService;

    private readonly FakeCaptureService _captureService;
    private readonly FakeRecognitionService _recognitionService;
    private readonly FakeTranslationService _translationService;
    private readonly FakePresentationService _presentationService;

    public PipelineRuntimeTests()
    {
        _artifactStore = new InMemoryArtifactStore();
        _mockLogger = new MockLogger();
        _mockTelemetry = new MockTelemetry();
        _textProcessorService = new TextProcessorService();

        _captureService = new FakeCaptureService();
        _recognitionService = new FakeRecognitionService();
        _translationService = new FakeTranslationService();
        _presentationService = new FakePresentationService();
    }

    [Fact]
    public async Task TriggerExecutionAsync_ShouldExecuteFullPipelineSuccessfully()
    {
        // Arrange
        _captureService.ResultPath = "mock_screen.png";
        _recognitionService.ResultText = "Welcome to Monolith";
        _translationService.ResultText = "Chào mừng tới Monolith";

        var runtime = new PipelineRuntime(
            _captureService,
            _recognitionService,
            _textProcessorService,
            _translationService,
            _presentationService,
            _artifactStore,
            _mockLogger,
            _mockTelemetry
        );

        var statusHistory = new List<WorkItemStatus>();
        runtime.WorkItemUpdated += item => statusHistory.Add(item.Status);

        // Act
        var result = await runtime.TriggerExecutionAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(WorkItemStatus.Completed, result.Status);
        Assert.NotNull(result.RawImagePath);
        Assert.Contains(result.Id.Value.ToString(), result.RawImagePath); // Chứa ID trong đường dẫn file ảnh tạm
        Assert.Equal("Welcome to Monolith", result.RawText);
        Assert.Equal("Chào mừng tới Monolith", result.TranslatedText);
        Assert.Null(result.ErrorMessage);
        Assert.NotNull(result.CompletedAt);

        // Xác nhận Presentation đã được gọi
        Assert.True(_presentationService.Presented);
        Assert.Equal("Chào mừng tới Monolith", _presentationService.PresentedText);

        // Xác nhận trạng thái được cập nhật theo đúng thứ tự logic
        Assert.Contains(WorkItemStatus.Created, statusHistory);
        Assert.Contains(WorkItemStatus.Capturing, statusHistory);
        Assert.Contains(WorkItemStatus.Captured, statusHistory);
        Assert.Contains(WorkItemStatus.Recognizing, statusHistory);
        Assert.Contains(WorkItemStatus.Recognized, statusHistory);
        Assert.Contains(WorkItemStatus.Translating, statusHistory);
        Assert.Contains(WorkItemStatus.Translated, statusHistory);
        Assert.Contains(WorkItemStatus.Presenting, statusHistory);
        Assert.Contains(WorkItemStatus.Completed, statusHistory);
    }

    [Fact]
    public async Task TriggerExecutionAsync_ShouldExitEarly_WhenNoTextRecognized()
    {
        // Arrange
        _captureService.ResultPath = "empty_screen.png";
        _recognitionService.ResultText = ""; // Không nhận diện được chữ nào

        var runtime = new PipelineRuntime(
            _captureService,
            _recognitionService,
            _textProcessorService,
            _translationService,
            _presentationService,
            _artifactStore,
            _mockLogger,
            _mockTelemetry
        );

        // Act
        var result = await runtime.TriggerExecutionAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(WorkItemStatus.Completed, result.Status);
        Assert.Equal("[Không phát hiện chữ]", result.TranslatedText);
        
        // Cực kỳ quan trọng: Translation và Presentation KHÔNG được gọi
        Assert.False(_translationService.Called);
        Assert.False(_presentationService.Presented);
    }

    [Fact]
    public async Task TriggerExecutionAsync_ShouldIsolateErrors_AndMarkAsFailed()
    {
        // Arrange
        _captureService.ResultPath = "mock_screen.png";
        _recognitionService.ExceptionToThrow = new InvalidOperationException("OCR Engine failure");

        var runtime = new PipelineRuntime(
            _captureService,
            _recognitionService,
            _textProcessorService,
            _translationService,
            _presentationService,
            _artifactStore,
            _mockLogger,
            _mockTelemetry
        );

        // Act
        // Lỗi không được ném ra ngoài mà được cô lập trong WorkItem
        var result = await runtime.TriggerExecutionAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(WorkItemStatus.Failed, result.Status);
        Assert.Equal("OCR Engine failure", result.ErrorMessage);
        Assert.NotNull(result.CompletedAt);

        // Dịch thuật và trình diễn không được gọi sau khi lỗi phát sinh
        Assert.False(_translationService.Called);
        Assert.False(_presentationService.Presented);
    }

    [Fact]
    public async Task TriggerExecutionAsync_ShouldCancelPreviousJob_WhenNewJobIsTriggered()
    {
        // Arrange - Setup Capture Service có delay để mô phỏng tác vụ đang chạy
        var delayedCapture = new DelayedFakeCaptureService(TimeSpan.FromMilliseconds(150));
        
        var runtime = new PipelineRuntime(
            delayedCapture,
            _recognitionService,
            _textProcessorService,
            _translationService,
            _presentationService,
            _artifactStore,
            _mockLogger,
            _mockTelemetry
        );

        // Act
        // Kích hoạt tác vụ 1 (chạy ngầm, không await ngay)
        var task1 = runtime.TriggerExecutionAsync();

        // Chờ 30ms để chắc chắn tác vụ 1 đã khởi chạy và đang trong giai đoạn capture
        await Task.Delay(30);

        // Kích hoạt tác vụ 2 ngay lập tức (Latest Wins)
        var task2 = runtime.TriggerExecutionAsync();

        // Chờ cả 2 tác vụ hoàn tất
        var res1 = await task1;
        var res2 = await task2;

        // Assert
        Assert.Equal(WorkItemStatus.Failed, res1.Status); // Tác vụ 1 bị hủy bỏ
        Assert.Equal("Operation was cancelled.", res1.ErrorMessage);
        
        Assert.Equal(WorkItemStatus.Completed, res2.Status); // Tác vụ 2 hoàn tất thành công
        Assert.Equal("Xin chào", res2.TranslatedText);
    }

    // Các class Mock/Fake phục vụ Test
    private class FakeCaptureService : ICaptureService
    {
        public string ResultPath { get; set; } = "captured.png";
        public Task<string> CaptureTargetWindowAsync(string outputFilePath, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ResultPath);
        }
    }

    private class DelayedFakeCaptureService : ICaptureService
    {
        private readonly TimeSpan _delay;

        public DelayedFakeCaptureService(TimeSpan delay)
        {
            _delay = delay;
        }

        public async Task<string> CaptureTargetWindowAsync(string outputFilePath, CancellationToken cancellationToken = default)
        {
            // Mô phỏng delay và phản hồi cancellationToken
            await Task.Delay(_delay, cancellationToken);
            return outputFilePath;
        }
    }

    private class FakeRecognitionService : IRecognitionService
    {
        public string ResultText { get; set; } = "Hello";
        public Exception? ExceptionToThrow { get; set; }
        public Task<OcrResultInfo> RecognizeTextAsync(string imagePath, CancellationToken cancellationToken = default)
        {
            if (ExceptionToThrow != null) throw ExceptionToThrow;
            var lines = new List<OcrLineInfo> { new OcrLineInfo(ResultText, 10, 20, 100, 30) };
            return Task.FromResult(new OcrResultInfo(ResultText, lines));
        }
    }

    private class FakeTranslationService : ITranslationService
    {
        public string ResultText { get; set; } = "Xin chào";
        public bool Called { get; private set; }
        public Task<string> TranslateTextAsync(string rawText, CancellationToken cancellationToken = default)
        {
            Called = true;
            return Task.FromResult(ResultText);
        }
    }

    private class FakePresentationService : IPresentationService
    {
        public bool Presented { get; private set; }
        public string? PresentedText { get; private set; }
        public Task PresentTranslationAsync(string translatedText, CancellationToken cancellationToken = default)
        {
            Presented = true;
            PresentedText = translatedText;
            return Task.CompletedTask;
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
