using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Crai.Application.Contracts.Runtime;
using Crai.Application.Contracts.Services;
using Crai.Application.Contracts.Infrastructure;
using Crai.Domain.Runtime;

namespace Crai.Runtime.Engine;

public class PipelineRuntime : IPipelineRuntime
{
    private readonly ICaptureService _captureService;
    private readonly IRecognitionService _recognitionService;
    private readonly ITextProcessorService _textProcessorService;
    private readonly ITranslationService _translationService;
    private readonly IPresentationService _presentationService;
    private readonly IArtifactStore _artifactStore;
    private readonly IStructuredLogger _logger;
    private readonly ITelemetryService _telemetry;
    
    private readonly object _runLock = new();
    private CancellationTokenSource? _activeCts;

    public event Action<WorkItem>? WorkItemUpdated;

    public PipelineRuntime(
        ICaptureService captureService,
        IRecognitionService recognitionService,
        ITextProcessorService textProcessorService,
        ITranslationService translationService,
        IPresentationService presentationService,
        IArtifactStore artifactStore,
        IStructuredLogger logger,
        ITelemetryService telemetry)
    {
        _captureService = captureService;
        _recognitionService = recognitionService;
        _textProcessorService = textProcessorService;
        _translationService = translationService;
        _presentationService = presentationService;
        _artifactStore = artifactStore;
        _logger = logger;
        _telemetry = telemetry;
    }

    public async Task<WorkItem> TriggerExecutionAsync(CancellationToken cancellationToken = default)
    {
        var workItem = new WorkItem();
        UpdateStatus(workItem, WorkItemStatus.Created);

        CancellationTokenSource linkedCts;
        lock (_runLock)
        {
            // Cơ chế LATEST WINS: Hủy bỏ frame dịch cũ đang chạy dở
            if (_activeCts != null)
            {
                _logger.LogInfo($"[PipelineRuntime] Yêu cầu dịch mới tới. Đang hủy bỏ tiến trình dịch cũ...");
                _activeCts.Cancel();
                _activeCts.Dispose();
            }

            // Tạo CTS liên kết mới cho tiến trình hiện tại
            linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _activeCts = linkedCts;
        }

        var token = linkedCts.Token;

        // Tạo file lưu trữ ảnh riêng biệt theo ID để tránh xung đột file IO giữa các frame song song
        var workItemImagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"runtime_capture_{workItem.Id.Value}.png");
        workItem.RawImagePath = workItemImagePath;

        // Bắt đầu đo đạc latency của toàn bộ luồng E2E
        using var e2eSpan = _telemetry.StartTrace("E2EPipeline_Total");
        _logger.LogInfo($"[PipelineRuntime] Khởi động E2E Pipeline (WorkItemId: {workItem.Id.Value})");

        try
        {
            token.ThrowIfCancellationRequested();

            // 1. Capture Step
            UpdateStatus(workItem, WorkItemStatus.Capturing);
            string capturedPath;
            using (var span = _telemetry.StartTrace("Pipeline_Capture"))
            {
                capturedPath = await _captureService.CaptureTargetWindowAsync(workItemImagePath, token);
            }
            UpdateStatus(workItem, WorkItemStatus.Captured);

            token.ThrowIfCancellationRequested();

            // 2. OCR / Recognition Step
            UpdateStatus(workItem, WorkItemStatus.Recognizing);
            string rawText;
            using (var span = _telemetry.StartTrace("Pipeline_Recognition"))
            {
                rawText = await _recognitionService.RecognizeTextAsync(capturedPath, token);
            }
            UpdateStatus(workItem, WorkItemStatus.Recognized);

            // Xử lý chuẩn hóa text (Text Processing) trước khi dịch thuật
            var normalizedText = _textProcessorService.NormalizeText(rawText);
            workItem.RawText = normalizedText;

            // Tối ưu hóa dừng sớm: Nếu không có chữ nào nhận dạng được, kết thúc luôn
            if (string.IsNullOrWhiteSpace(normalizedText))
            {
                _logger.LogInfo($"[PipelineRuntime] Không phát hiện văn bản ở WorkItemId '{workItem.Id.Value}'. Kết thúc sớm.");
                workItem.MarkAsCompleted("[Không phát hiện chữ]");
                UpdateStatus(workItem, WorkItemStatus.Completed);
                TryDeleteFile(workItemImagePath);
                return workItem;
            }

            token.ThrowIfCancellationRequested();

            // 3. Translation Step
            UpdateStatus(workItem, WorkItemStatus.Translating);
            string translatedText;
            using (var span = _telemetry.StartTrace("Pipeline_Translation"))
            {
                translatedText = await _translationService.TranslateTextAsync(normalizedText, token);
            }
            workItem.TranslatedText = translatedText;
            UpdateStatus(workItem, WorkItemStatus.Translated);

            token.ThrowIfCancellationRequested();

            // 4. Presentation Step
            UpdateStatus(workItem, WorkItemStatus.Presenting);
            using (var span = _telemetry.StartTrace("Pipeline_Presentation"))
            {
                await _presentationService.PresentTranslationAsync(translatedText, token);
            }
            
            // Hoàn tất luồng E2E
            workItem.MarkAsCompleted(translatedText);
            UpdateStatus(workItem, WorkItemStatus.Completed);
            
            _logger.LogInfo($"[PipelineRuntime] WorkItemId '{workItem.Id.Value}' hoàn tất thành công trong {e2eSpan.ElapsedMilliseconds} ms.");
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning($"[PipelineRuntime] Tiến trình xử lý WorkItemId '{workItem.Id.Value}' đã bị hủy bỏ (Latest Wins / Canceled).");
            workItem.MarkAsFailed("Operation was cancelled.");
            UpdateStatus(workItem, WorkItemStatus.Failed);
        }
        catch (Exception ex)
        {
            _logger.LogError($"[PipelineRuntime] Lỗi thực thi pipeline ở WorkItemId '{workItem.Id.Value}': {ex.Message}", ex);
            workItem.MarkAsFailed(ex.Message);
            UpdateStatus(workItem, WorkItemStatus.Failed);
        }
        finally
        {
            // Dọn dẹp file ảnh tạm sau khi xử lý xong (hoặc lỗi)
            TryDeleteFile(workItemImagePath);

            lock (_runLock)
            {
                // Chỉ gỡ bỏ _activeCts nếu chính nó là active cts hiện tại
                if (_activeCts == linkedCts)
                {
                    _activeCts = null;
                }
            }
            linkedCts.Dispose();
        }

        return workItem;
    }

    public void Stop()
    {
        lock (_runLock)
        {
            if (_activeCts != null)
            {
                _activeCts.Cancel();
                _activeCts.Dispose();
                _activeCts = null;
            }
        }
        _logger.LogDebug("[PipelineRuntime] Đã yêu cầu dừng toàn bộ các tiến trình hoạt động.");
    }

    private void UpdateStatus(WorkItem item, WorkItemStatus status)
    {
        item.Status = status;
        _artifactStore.SaveWorkItem(item);
        
        try
        {
            WorkItemUpdated?.Invoke(item);
        }
        catch (Exception ex)
        {
            _logger.LogError($"[PipelineRuntime] Lỗi phát sự kiện WorkItemUpdated: {ex.Message}", ex);
        }
    }

    private void TryDeleteFile(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"[PipelineRuntime] Không thể xóa file tạm '{path}': {ex.Message}");
        }
    }
}
