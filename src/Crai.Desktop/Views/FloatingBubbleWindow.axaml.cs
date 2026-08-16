using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Microsoft.Extensions.DependencyInjection;
using Crai.Application.Contracts.Runtime;
using Crai.Application.Contracts.Services;
using Crai.Application.Contracts.Infrastructure;
using Crai.Domain.Runtime;
using Crai.Desktop.Feasibility;
using Crai.Modules.Translation.Services;

namespace Crai.Desktop.Views;

public partial class FloatingBubbleWindow : Window
{
    private readonly IPipelineRuntime _pipelineRuntime;
    private readonly IConfigurationService _configService;
    private readonly ISecretManager _secretManager;
    private readonly ITranslationCache _translationCache;
    private readonly IStructuredLogger _logger;
    private GlobalHotkeyProto? _hotkeyProto;

    // MenuItems lưu trữ để thay đổi text động
    private MenuItem? _itemGoogle;
    private MenuItem? _itemGemini;

    // Các biến phục vụ drag-and-drop
    private bool _isDragging;
    private PixelPoint _positionBeforeDrag;
    private Point _pointerPositionBeforeDrag;
    private bool _hasMovedSignificantly;

    public FloatingBubbleWindow()
    {
        InitializeComponent();

        // Lấy dependencies từ DI Container
        _pipelineRuntime = CompositionRoot.ServiceProvider.GetRequiredService<IPipelineRuntime>();
        _configService = CompositionRoot.ServiceProvider.GetRequiredService<IConfigurationService>();
        _secretManager = CompositionRoot.ServiceProvider.GetRequiredService<ISecretManager>();
        _translationCache = CompositionRoot.ServiceProvider.GetRequiredService<ITranslationCache>();
        _logger = CompositionRoot.ServiceProvider.GetRequiredService<IStructuredLogger>();

        // Đăng ký sự kiện cập nhật trạng thái
        _pipelineRuntime.WorkItemUpdated += OnWorkItemUpdated;

        // Đăng ký phím tắt toàn cục Ctrl+Shift+T
        _hotkeyProto = new GlobalHotkeyProto(this);
        _hotkeyProto.HotkeyTriggered += async () => await StartOverlayTranslationAsync();

        Opened += FloatingBubbleWindow_Opened;
        Closed += FloatingBubbleWindow_Closed;

        // Gán ContextMenu để click chuột phải hiển thị
        InitializeContextMenu();
    }

    private void FloatingBubbleWindow_Opened(object? sender, EventArgs e)
    {
        // Dock về góc phải giữa màn hình
        DockToRightCenter();
    }

    private void FloatingBubbleWindow_Closed(object? sender, EventArgs e)
    {
        _hotkeyProto?.Dispose();
        _hotkeyProto = null;
    }

    private void DockToRightCenter()
    {
        var screen = Screens.Primary;
        if (screen is null) return;

        var wa = screen.WorkingArea;
        double scaling = screen.Scaling;

        // Định vị trí nút nổi ở cạnh phải màn hình, nằm ở 1/3 chiều cao
        Position = new PixelPoint(
            (int)(wa.X + wa.Width - (Width * scaling) - 20),
            (int)(wa.Y + (wa.Height / 3))
        );
    }

    // --- Xử lý Drag & Drop ---
    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var properties = e.GetCurrentPoint(this).Properties;

        if (properties.IsLeftButtonPressed)
        {
            _isDragging = true;
            _hasMovedSignificantly = false;
            _positionBeforeDrag = Position;
            _pointerPositionBeforeDrag = e.GetPosition(this);
            e.Handled = true;
        }
        else if (properties.IsRightButtonPressed)
        {
            // Click chuột phải hiển thị menu cài đặt
            ContextMenu?.Open(BubbleBorder);
            e.Handled = true;
        }
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isDragging)
        {
            var currentPointerPosition = e.GetPosition(this);
            var delta = currentPointerPosition - _pointerPositionBeforeDrag;

            if (Math.Abs(delta.X) > 4 || Math.Abs(delta.Y) > 4)
            {
                _hasMovedSignificantly = true;
            }

            var screen = Screens.ScreenFromPoint(Position);
            double scaling = screen?.Scaling ?? 1.0;

            Position = new PixelPoint(
                _positionBeforeDrag.X + (int)(delta.X * scaling),
                _positionBeforeDrag.Y + (int)(delta.Y * scaling)
            );

            // Cập nhật lại vị trí gốc drag mượt mà
            _positionBeforeDrag = Position;
        }
    }

    private async void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_isDragging)
        {
            _isDragging = false;
            e.Handled = true;

            // Nếu người dùng chỉ click chuột (không di chuyển đáng kể) -> Kích hoạt dịch
            if (!_hasMovedSignificantly)
            {
                await StartOverlayTranslationAsync();
            }
        }
    }

    // --- Kích hoạt E2E Pipeline Dịch Đè (Overlay) ---
    private async Task StartOverlayTranslationAsync()
    {
        _logger.LogInfo("[FloatingBubble] Bắt đầu chạy tiến trình dịch màn hình...");
        
        // Ẩn nút nổi để tránh che khuất hình ảnh game/truyện khi chụp màn hình
        Hide();

        // Chờ 100ms để hệ thống render cập nhật giao diện (ẩn nút nổi hoàn toàn trên desktop)
        await Task.Delay(100);

        try
        {
            var workItem = await _pipelineRuntime.TriggerExecutionAsync();

            if (workItem.Status == WorkItemStatus.Completed && workItem.OcrResult is OcrResultInfo ocrResult)
            {
                // Mở cửa sổ Overlay hiển thị chữ đè lên màn hình
                var overlay = new TranslationOverlayWindow();
                overlay.RenderTranslations(ocrResult.Lines);
                overlay.Show();
            }
            else if (workItem.Status == WorkItemStatus.Completed)
            {
                _logger.LogInfo("[FloatingBubble] Dịch xong nhưng không phát hiện chữ nào.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"[FloatingBubble] Lỗi dịch màn hình: {ex.Message}", ex);
        }
        finally
        {
            // Trở lại hiển thị nút nổi sau khi chụp/dịch xong
            Show();
            BubbleBorder.BorderBrush = new SolidColorBrush(Colors.White);
        }
    }

    private void OnWorkItemUpdated(WorkItem item)
    {
        // Thay đổi màu nút nổi động theo trạng thái của pipeline
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            switch (item.Status)
            {
                case WorkItemStatus.Capturing:
                case WorkItemStatus.Recognizing:
                    BubbleBorder.BorderBrush = new SolidColorBrush(Color.Parse("#FF3A86FF")); // Xanh lam
                    break;
                case WorkItemStatus.Translating:
                    BubbleBorder.BorderBrush = new SolidColorBrush(Color.Parse("#FF8338EC")); // Tím
                    break;
                case WorkItemStatus.Completed:
                    BubbleBorder.BorderBrush = new SolidColorBrush(Colors.White);
                    break;
                case WorkItemStatus.Failed:
                    BubbleBorder.BorderBrush = new SolidColorBrush(Color.Parse("#FFFF5555")); // Đỏ (báo lỗi)
                    break;
            }
        });
    }

    // --- Khởi tạo Menu Cài đặt chuột phải (ContextMenu) ---
    private void InitializeContextMenu()
    {
        var menu = new ContextMenu();

        // 1. Dịch màn hình
        var itemTranslate = new MenuItem { Header = "⚡ Dịch Màn Kinh (Ctrl+Shift+T)" };
        itemTranslate.Click += async (s, e) => await StartOverlayTranslationAsync();
        menu.Items.Add(itemTranslate);

        menu.Items.Add(new Separator());

        // 2. Lựa chọn Engine Dịch
        var itemEngine = new MenuItem { Header = "⚙️ Đổi Engine dịch" };
        
        _itemGoogle = new MenuItem();
        _itemGemini = new MenuItem();

        UpdateEngineMenuHeaders();

        _itemGoogle.Click += (s, e) =>
        {
            TranslationRouter.PreferredEngineOverride = "GoogleTranslate";
            UpdateEngineMenuHeaders();
            _logger.LogInfo("[FloatingBubble] Đã đổi Engine dịch thành Google Translate.");
        };

        _itemGemini.Click += (s, e) =>
        {
            TranslationRouter.PreferredEngineOverride = "Gemini";
            UpdateEngineMenuHeaders();
            _logger.LogInfo("[FloatingBubble] Đã đổi Engine dịch thành Gemini AI.");
        };

        itemEngine.Items.Add(_itemGoogle);
        itemEngine.Items.Add(_itemGemini);
        menu.Items.Add(itemEngine);

        // 3. Xóa cache SQLite
        var itemClearCache = new MenuItem { Header = "🧹 Dọn dẹp Cache dịch thuật" };
        itemClearCache.Click += async (s, e) =>
        {
            await _translationCache.ClearAsync();
            _logger.LogInfo("[FloatingBubble] Đã dọn dẹp sạch cache dịch thuật.");
        };
        menu.Items.Add(itemClearCache);

        menu.Items.Add(new Separator());

        // 4. Thoát ứng dụng
        var itemExit = new MenuItem { Header = "✕ Thoát CRAI" };
        itemExit.Click += (s, e) => Close();
        menu.Items.Add(itemExit);

        ContextMenu = menu;
    }

    private void UpdateEngineMenuHeaders()
    {
        if (_itemGoogle == null || _itemGemini == null) return;

        var currentEngine = TranslationRouter.PreferredEngineOverride ?? 
                            _configService.GetValue<string>("Translation:Engine") ?? 
                            "GoogleTranslate";

        if (currentEngine.Equals("Gemini", StringComparison.OrdinalIgnoreCase))
        {
            _itemGoogle.Header = "   Google Translate (Mặc định)";
            _itemGemini.Header = "✓ Gemini AI (Glossary)";
        }
        else
        {
            _itemGoogle.Header = "✓ Google Translate (Mặc định)";
            _itemGemini.Header = "   Gemini AI (Glossary)";
        }
    }
}
