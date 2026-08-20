using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
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
    private MenuItem? _itemMergeLines;
    private MenuItem? _itemContinuous;
    private readonly List<MenuItem> _durationMenuItems = new();
    private TranslationOverlayWindow? _activeOverlay;
    private CancellationTokenSource? _continuousCts;

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

        var isContinuous = _configService.GetValue<bool>("Translation:Continuous");
        if (isContinuous)
        {
            StartContinuousTranslation();
        }
    }

    private void FloatingBubbleWindow_Closed(object? sender, EventArgs e)
    {
        _hotkeyProto?.Dispose();
        _hotkeyProto = null;

        StopContinuousTranslation();

        _activeOverlay?.Close();
        _activeOverlay = null;
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
            BubbleBorder.ContextMenu?.Open(BubbleBorder);
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
        
        bool isContinuous = _configService.GetValue<bool>("Translation:Continuous");
        
        // Chỉ ẩn nút nổi nếu không ở chế độ dịch liên tục
        if (!isContinuous)
        {
            Hide();
            // Chờ 100ms để hệ thống render cập nhật giao diện (ẩn nút nổi hoàn toàn trên desktop)
            await Task.Delay(100);
        }

        try
        {
            var workItem = await _pipelineRuntime.TriggerExecutionAsync();

            if (workItem.Status == WorkItemStatus.Completed && workItem.OcrResult is OcrResultInfo ocrResult && ocrResult.Lines.Count > 0)
            {
                // Đóng Overlay cũ trước khi mở Overlay mới
                _activeOverlay?.Close();

                // Đọc cấu hình thời gian hiển thị
                var duration = _configService.GetValue<int>("Translation:OverlayDuration");
                if (duration == 0) duration = 8; // Mặc định 8 giây nếu chưa cấu hình

                // Mở cửa sổ Overlay hiển thị chữ đè lên màn hình
                _activeOverlay = new TranslationOverlayWindow(duration);
                _activeOverlay.RenderTranslations(ocrResult.Lines);
                _activeOverlay.Show();
            }
            else if (workItem.Status == WorkItemStatus.Completed)
            {
                _logger.LogInfo("[FloatingBubble] Dịch xong nhưng không phát hiện chữ nào.");
                // Tự động đóng overlay cũ khi không còn chữ
                _activeOverlay?.Close();
                _activeOverlay = null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"[FloatingBubble] Lỗi dịch màn hình: {ex.Message}", ex);
        }
        finally
        {
            // Trở lại hiển thị nút nổi nếu đã ẩn
            if (!isContinuous)
            {
                Show();
            }
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
        var itemTranslate = new MenuItem { Header = "⚡ Dịch Màn Hình (Ctrl+Shift+T)" };
        itemTranslate.Click += async (s, e) => await StartOverlayTranslationAsync();
        menu.Items.Add(itemTranslate);

        // 1.5 Dịch gộp câu (MergeLines)
        _itemMergeLines = new MenuItem();
        UpdateMergeLinesMenuHeader();
        _itemMergeLines.Click += (s, e) =>
        {
            var currentValue = _configService.GetValue<bool>("Translation:MergeLines");
            _configService.UpdateValue("Translation:MergeLines", !currentValue);
            UpdateMergeLinesMenuHeader();
            _logger.LogInfo($"[FloatingBubble] Đã đổi chế độ dịch gộp câu thành: {!currentValue}.");
        };
        menu.Items.Add(_itemMergeLines);

        // 1.6 Dịch liên tục (Continuous)
        _itemContinuous = new MenuItem();
        UpdateContinuousMenuHeader();
        _itemContinuous.Click += (s, e) =>
        {
            var currentValue = _configService.GetValue<bool>("Translation:Continuous");
            var newValue = !currentValue;
            _configService.UpdateValue("Translation:Continuous", newValue);
            UpdateContinuousMenuHeader();
            _logger.LogInfo($"[FloatingBubble] Đã đổi chế độ dịch liên tục thành: {newValue}.");

            if (newValue)
            {
                StartContinuousTranslation();
            }
            else
            {
                StopContinuousTranslation();
            }
        };
        menu.Items.Add(_itemContinuous);

        // 1.7 Thời gian hiển thị bản dịch (Submenu)
        var itemDuration = new MenuItem { Header = "⏱️ Thời gian hiển thị bản dịch" };
        var durations = new (int value, string label)[]
        {
            (5, "5 giây"),
            (8, "8 giây (Mặc định)"),
            (15, "15 giây"),
            (30, "30 giây"),
            (60, "60 giây"),
            (-1, "Không tự tắt (Vô hạn)")
        };

        _durationMenuItems.Clear();
        foreach (var dur in durations)
        {
            var item = new MenuItem { Header = dur.label, Tag = dur.value };
            item.Click += (s, e) =>
            {
                var val = (int)((MenuItem)s!).Tag!;
                _configService.UpdateValue("Translation:OverlayDuration", val);
                UpdateDurationMenuHeaders();
                _logger.LogInfo($"[FloatingBubble] Đã đổi thời gian hiển thị bản dịch thành: {val} giây.");
            };
            itemDuration.Items.Add(item);
            _durationMenuItems.Add(item);
        }
        menu.Items.Add(itemDuration);
        UpdateDurationMenuHeaders();

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

        BubbleBorder.ContextMenu = menu;
    }

    private void UpdateMergeLinesMenuHeader()
    {
        if (_itemMergeLines == null) return;
        var mergeLines = _configService.GetValue<bool>("Translation:MergeLines");
        if (mergeLines)
        {
            _itemMergeLines.Header = "✓ Dịch gộp câu (Không ngắt dòng)";
        }
        else
        {
            _itemMergeLines.Header = "   Dịch gộp câu (Không ngắt dòng)";
        }
    }

    private void UpdateContinuousMenuHeader()
    {
        if (_itemContinuous == null) return;
        var isContinuous = _configService.GetValue<bool>("Translation:Continuous");
        if (isContinuous)
        {
            _itemContinuous.Header = "✓ Dịch liên tục (Tự động)";
        }
        else
        {
            _itemContinuous.Header = "   Dịch liên tục (Tự động)";
        }
    }

    private void UpdateDurationMenuHeaders()
    {
        var currentDuration = _configService.GetValue<int>("Translation:OverlayDuration");
        if (currentDuration == 0) currentDuration = 8;

        foreach (var item in _durationMenuItems)
        {
            var val = (int)item.Tag!;
            var label = val == 5 ? "5 giây" :
                        val == 8 ? "8 giây (Mặc định)" :
                        val == 15 ? "15 giây" :
                        val == 30 ? "30 giây" :
                        val == 60 ? "60 giây" :
                        val == -1 ? "Không tự tắt (Vô hạn)" : "";

            if (val == currentDuration)
            {
                item.Header = "✓ " + label;
            }
            else
            {
                item.Header = "   " + label;
            }
        }
    }

    private void StartContinuousTranslation()
    {
        StopContinuousTranslation();

        _continuousCts = new CancellationTokenSource();
        var token = _continuousCts.Token;

        Task.Run(async () =>
        {
            while (!token.IsCancellationRequested)
            {
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await StartOverlayTranslationAsync();
                });

                var delayMs = _configService.GetValue<int>("Translation:ContinuousDelayMs");
                if (delayMs <= 0) delayMs = 1000;

                try
                {
                    await Task.Delay(delayMs, token);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }, token);
    }

    private void StopContinuousTranslation()
    {
        if (_continuousCts != null)
        {
            _continuousCts.Cancel();
            _continuousCts.Dispose();
            _continuousCts = null;
        }
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
