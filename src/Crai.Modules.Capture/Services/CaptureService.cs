using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
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
            // Thực thi chụp màn hình trên UI Thread của Avalonia để đọc tọa độ màn hình an toàn
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Lấy màn hình chính chứa cửa sổ (hoặc màn hình Primary làm mặc định)
                var screen = window.Screens.Primary ?? window.Screens.All[0];
                var bounds = screen.Bounds; // Tọa độ pixel thực tế của màn hình (Width x Height)

                _logger.LogDebug($"[CaptureService] Đang chụp toàn màn hình Desktop. Tọa độ: X={bounds.X}, Y={bounds.Y}, Size={bounds.Width}x{bounds.Height}");

                // Chụp màn hình Desktop bằng API System.Drawing.Common (CopyFromScreen)
                using (var bitmap = new System.Drawing.Bitmap(bounds.Width, bounds.Height))
                {
                    using (var g = System.Drawing.Graphics.FromImage(bitmap))
                    {
                        g.CopyFromScreen(bounds.X, bounds.Y, 0, 0, new System.Drawing.Size(bounds.Width, bounds.Height));
                    }

                    // Đảm bảo thư mục đầu ra tồn tại
                    var dir = Path.GetDirectoryName(outputFilePath);
                    if (dir != null && !Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }

                    // Xóa file cũ nếu có trước khi ghi đè
                    if (File.Exists(outputFilePath))
                    {
                        File.Delete(outputFilePath);
                    }

                    bitmap.Save(outputFilePath, System.Drawing.Imaging.ImageFormat.Png);
                }
            }, DispatcherPriority.Render);

            _logger.LogDebug($"[CaptureService] Đã chụp toàn màn hình Desktop thành công tại: {outputFilePath}");
            return outputFilePath;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError($"[CaptureService] Lỗi khi chụp toàn màn hình Desktop: {ex.Message}", ex);
            throw new InvalidOperationException($"Lỗi khi chụp toàn màn hình Desktop: {ex.Message}", ex);
        }
    }
}
