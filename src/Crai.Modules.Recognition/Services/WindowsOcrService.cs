using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media.Ocr;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Globalization;
using Crai.Application.Contracts.Services;
using Crai.Application.Contracts.Infrastructure;

namespace Crai.Modules.Recognition.Services;

public class WindowsOcrService : IRecognitionService
{
    private readonly IConfigurationService _configService;
    private readonly IStructuredLogger _logger;

    public WindowsOcrService(IConfigurationService configService, IStructuredLogger logger)
    {
        _configService = configService ?? throw new ArgumentNullException(nameof(configService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<OcrResultInfo> RecognizeTextAsync(string imagePath, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
        {
            _logger.LogWarning($"[WindowsOcrService] File ảnh nguồn không tồn tại hoặc đường dẫn trống: '{imagePath}'");
            return new OcrResultInfo(string.Empty, new List<OcrLineInfo>());
        }

        try
        {
            // 1. Tạo OcrEngine dựa trên ngôn ngữ cấu hình
            var defaultLang = _configService.GetValue<string>("OCR:DefaultLanguage") ?? "en-US";
            var targetLanguage = new Language(defaultLang);
            
            // Kiểm tra xem hệ thống có hỗ trợ ngôn ngữ này không
            if (!OcrEngine.IsLanguageSupported(targetLanguage))
            {
                _logger.LogWarning($"[WindowsOcrService] Hệ điều hành Windows hiện tại chưa được cài đặt Language Pack cho ngôn ngữ: '{defaultLang}'. Fallback về ngôn ngữ mặc định đầu tiên của hệ thống.");
            }

            var ocrEngine = OcrEngine.TryCreateFromLanguage(targetLanguage);
            if (ocrEngine == null)
            {
                // Fallback tạo OcrEngine mặc định của User
                ocrEngine = OcrEngine.TryCreateFromLanguage(new Language("en-US"));
                if (ocrEngine == null)
                 {
                    throw new InvalidOperationException("Không thể tạo Windows OcrEngine cho cả ngôn ngữ chỉ định lẫn en-US.");
                }
            }

            // 2. Load ảnh thô bằng Windows Storage và Graphics Imaging (WinRT)
            var storageFile = await StorageFile.GetFileFromPathAsync(imagePath);
            using var stream = await storageFile.OpenAsync(FileAccessMode.Read);
            
            var decoder = await BitmapDecoder.CreateAsync(stream);
            using var softwareBitmap = await decoder.GetSoftwareBitmapAsync();

            cancellationToken.ThrowIfCancellationRequested();

            // 3. Thực hiện nhận diện
            _logger.LogDebug($"[WindowsOcrService] Đang quét chữ (OCR) bằng Windows Media Engine (Ngôn ngữ: {ocrEngine.RecognizerLanguage.LanguageTag})...");
            var ocrResult = await ocrEngine.RecognizeAsync(softwareBitmap);

            var recognizedText = ocrResult.Text ?? string.Empty;
            _logger.LogDebug($"[WindowsOcrService] OCR thành công. Nhận diện được {recognizedText.Length} ký tự.");

            // 4. Tính toán tọa độ Bounding Box của từng dòng bằng cách gộp BoundingRect các Words
            var linesList = new List<OcrLineInfo>();
            foreach (var line in ocrResult.Lines)
            {
                if (line.Words == null || line.Words.Count == 0) continue;

                double minX = double.MaxValue;
                double minY = double.MaxValue;
                double maxX = double.MinValue;
                double maxY = double.MinValue;

                foreach (var word in line.Words)
                {
                    var rect = word.BoundingRect;
                    if (rect.Left < minX) minX = rect.Left;
                    if (rect.Top < minY) minY = rect.Top;
                    if (rect.Right > maxX) maxX = rect.Right;
                    if (rect.Bottom > maxY) maxY = rect.Bottom;
                }

                var lineText = line.Text ?? string.Empty;
                linesList.Add(new OcrLineInfo(lineText, minX, minY, maxX - minX, maxY - minY));
            }

            return new OcrResultInfo(recognizedText, linesList);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError($"[WindowsOcrService] Lỗi khi nhận diện chữ (OCR): {ex.Message}", ex);
            throw new InvalidOperationException($"Lỗi khi quét chữ: {ex.Message}", ex);
        }
    }
}
