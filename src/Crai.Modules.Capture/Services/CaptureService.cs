using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Crai.Application.Contracts.Services;
using Crai.Application.Contracts.Infrastructure;

namespace Crai.Modules.Capture.Services;

public class CaptureService : ICaptureService
{
    private readonly ITargetWindowProvider _windowProvider;
    private readonly IStructuredLogger _logger;

    public CaptureService(ITargetWindowProvider windowProvider, IStructuredLogger logger)
    {
        _windowProvider = windowProvider ?? throw new ArgumentNullException(nameof(windowProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> CaptureTargetWindowAsync(string outputFilePath, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var targetObj = _windowProvider.GetTargetWindow();
        if (targetObj == null)
        {
            throw new InvalidOperationException("[CaptureService] Window mục tiêu chưa được thiết lập (Target Window is null).");
        }

        if (targetObj is not Window window)
        {
            throw new InvalidOperationException($"[CaptureService] Window mục tiêu có kiểu '{targetObj.GetType().Name}', không phải Avalonia.Controls.Window.");
        }

        try
        {
            // Thực thi trên UI Thread của Avalonia để tránh cross-thread exception
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                var bounds = window.Bounds;
                var scaling = window.Screens.Primary?.Scaling ?? 1.0;
                var pixelSize = new PixelSize((int)(bounds.Width * scaling), (int)(bounds.Height * scaling));
                var dpi = new Vector(96 * scaling, 96 * scaling);

                _logger.LogDebug($"[CaptureService] Đang chụp cửa sổ. Size: {bounds.Width}x{bounds.Height}, Scaling: {scaling}");

                using var bitmap = new RenderTargetBitmap(pixelSize, dpi);
                bitmap.Render(window);
                
                // Đảm bảo thư mục đầu ra tồn tại
                var dir = Path.GetDirectoryName(outputFilePath);
                if (dir != null && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                // Xóa file cũ nếu có trước khi save
                if (File.Exists(outputFilePath))
                {
                    File.Delete(outputFilePath);
                }

                // Sử dụng overload BitmapEncoderOptions mới để tránh warning obsolete
                bitmap.Save(outputFilePath);
            }, DispatcherPriority.Render);

            _logger.LogDebug($"[CaptureService] Đã chụp và lưu thành công ảnh chụp màn hình tại: {outputFilePath}");
            return outputFilePath;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError($"[CaptureService] Lỗi khi chụp màn hình cửa sổ: {ex.Message}", ex);
            throw new InvalidOperationException($"Lỗi khi chụp màn hình cửa sổ: {ex.Message}", ex);
        }
    }
}
