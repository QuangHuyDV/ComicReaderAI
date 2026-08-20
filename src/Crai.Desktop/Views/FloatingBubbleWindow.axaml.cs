using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Microsoft.Extensions.DependencyInjection;
using Crai.Application.Contracts.Runtime;
using Crai.Application.Contracts.Services;
using Crai.Application.Contracts.Infrastructure;
using Crai.Domain.Runtime;
using Crai.Desktop.Feasibility;
using Crai.Desktop.ViewModels;
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
    private readonly List<MenuItem> _presentationModeMenuItems = new();
    private MainWindow? _sidePanel;
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

        // Di chuyển Gemini API Key từ cấu hình thô sang Secret Manager an toàn (nếu có)
        var configKey = _configService.GetValue<string>("Translation:GeminiApiKey");
        if (!string.IsNullOrWhiteSpace(configKey))
        {
            try
            {
                _secretManager.StoreSecret("GeminiApiKey", configKey);
                _configService.UpdateValue("Translation:GeminiApiKey", "");
                _logger.LogInfo("[SecretManager] Đã mã hóa và di chuyển thành công GeminiApiKey từ appsettings.json vào Windows DPAPI Secret Manager.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"[SecretManager] Lỗi di chuyển GeminiApiKey: {ex.Message}", ex);
            }
        }

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

        var presentationMode = _configService.GetValue<string>("Translation:PresentationMode") ?? "Overlay";
        if (presentationMode.Equals("SidePanel", StringComparison.OrdinalIgnoreCase))
        {
            OpenSidePanel();
        }
    }

    private void FloatingBubbleWindow_Closed(object? sender, EventArgs e)
    {
        _hotkeyProto?.Dispose();
        _hotkeyProto = null;

        StopContinuousTranslation();

        _activeOverlay?.Close();
        _activeOverlay = null;

        CloseSidePanel();
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
        
        // 1. Ẩn nút nổi (nếu không ở chế độ dịch liên tục)
        if (!isContinuous)
        {
            Hide();
        }

        // 2. Tạm ẩn overlay đang hiển thị để tránh tự chụp đè bản dịch cũ
        _activeOverlay?.Hide();

        // Chờ 120ms để hệ thống render cập nhật ẩn các window trên desktop
        await Task.Delay(120);

        try
        {
            var workItem = await _pipelineRuntime.TriggerExecutionAsync();

            if (workItem.Status == WorkItemStatus.Completed && workItem.OcrResult is OcrResultInfo ocrResult && ocrResult.Lines.Count > 0)
            {
                var mode = _configService.GetValue<string>("Translation:PresentationMode") ?? "Overlay";

                if (mode.Equals("SidePanel", StringComparison.OrdinalIgnoreCase))
                {
                    // Bản dịch đã tự động được render bên trong Side Panel nhờ ViewModel binding
                    _activeOverlay?.Close();
                    _activeOverlay = null;
                }
                else
                {
                    // Đóng Overlay cũ trước khi mở Overlay mới
                    _activeOverlay?.Close();

                    // Đọc cấu hình thời gian hiển thị
                    var duration = _configService.GetValue<int>("Translation:OverlayDuration");
                    if (duration == 0) duration = 8; // Mặc định 8 giây nếu chưa cấu hình

                    // Mở cửa sổ Overlay hiển thị chữ đè lên màn hình
                    _activeOverlay = new TranslationOverlayWindow(duration);
                    _activeOverlay.RenderTranslations(ocrResult.Lines, mode.Equals("OverlayTranslucent", StringComparison.OrdinalIgnoreCase));
                    _activeOverlay.Show();
                }
            }
            else if (workItem.Status == WorkItemStatus.Completed)
            {
                _logger.LogInfo("[FloatingBubble] Dịch xong nhưng không phát hiện chữ nào.");
                // Tự động đóng overlay cũ khi không còn chữ
                _activeOverlay?.Close();
                _activeOverlay = null;
            }
            else
            {
                // Nếu pipeline thất bại, khôi phục hiển thị overlay cũ
                _activeOverlay?.Show();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"[FloatingBubble] Lỗi dịch màn hình: {ex.Message}", ex);
            // Khôi phục hiển thị overlay cũ nếu gặp lỗi
            _activeOverlay?.Show();
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

        // 0. Đóng bản dịch đang hiển thị
        var itemCloseOverlay = new MenuItem { Header = "✕ Đóng bản dịch đang hiển thị" };
        itemCloseOverlay.Click += (s, e) =>
        {
            _activeOverlay?.Close();
            _activeOverlay = null;
        };
        menu.Items.Add(itemCloseOverlay);

        menu.Items.Add(new Separator());

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

        // 1.8 Cấu hình Gemini API Key
        var itemApiKey = new MenuItem { Header = "🔑 Cấu hình Gemini API Key..." };
        itemApiKey.Click += async (s, e) =>
        {
            await PromptForGeminiApiKeyAsync();
        };
        menu.Items.Add(itemApiKey);

        // 1.9 Kiểu hiển thị bản dịch (Submenu)
        var itemPresentation = new MenuItem { Header = "👁️ Kiểu hiển thị bản dịch" };
        var modes = new (string value, string label)[]
        {
            ("Overlay", "Dịch đè màn hình (Che 100%)"),
            ("OverlayTranslucent", "Dịch đè màn hình (Nhìn xuyên nền)"),
            ("SidePanel", "Bảng phụ bên cạnh (Side Panel)")
        };

        _presentationModeMenuItems.Clear();
        foreach (var m in modes)
        {
            var item = new MenuItem { Header = m.label, Tag = m.value };
            item.Click += (s, e) =>
            {
                var val = (string)((MenuItem)s!).Tag!;
                _configService.UpdateValue("Translation:PresentationMode", val);
                UpdatePresentationModeMenuHeaders();
                _logger.LogInfo($"[FloatingBubble] Đã đổi kiểu hiển thị bản dịch thành: {val}.");

                if (val.Equals("SidePanel", StringComparison.OrdinalIgnoreCase))
                {
                    OpenSidePanel();
                }
                else
                {
                    CloseSidePanel();
                }
            };
            itemPresentation.Items.Add(item);
            _presentationModeMenuItems.Add(item);
        }
        menu.Items.Add(itemPresentation);
        UpdatePresentationModeMenuHeaders();

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

    private void UpdatePresentationModeMenuHeaders()
    {
        var currentMode = _configService.GetValue<string>("Translation:PresentationMode") ?? "Overlay";

        foreach (var item in _presentationModeMenuItems)
        {
            var val = (string)item.Tag!;
            var label = val == "Overlay" ? "Dịch đè màn hình (Che 100%)" :
                        val == "OverlayTranslucent" ? "Dịch đè màn hình (Nhìn xuyên nền)" :
                        val == "SidePanel" ? "Bảng phụ bên cạnh (Side Panel)" : "";

            if (val.Equals(currentMode, StringComparison.OrdinalIgnoreCase))
            {
                item.Header = "✓ " + label;
            }
            else
            {
                item.Header = "   " + label;
            }
        }
    }

    private void OpenSidePanel()
    {
        if (_sidePanel == null)
        {
            var vm = CompositionRoot.ServiceProvider.GetRequiredService<MainViewModel>();
            _sidePanel = new MainWindow(registerHotkey: false)
            {
                DataContext = vm
            };
            _sidePanel.Closed += (s, e) => _sidePanel = null;
            _sidePanel.Show();
        }
        else
        {
            _sidePanel.Activate();
        }
    }

    private void CloseSidePanel()
    {
        if (_sidePanel != null)
        {
            _sidePanel.Close();
            _sidePanel = null;
        }
    }

    private async Task PromptForGeminiApiKeyAsync()
    {
        var inputWindow = new Window
        {
            Title = "Cấu hình Gemini API Key",
            Width = 450,
            Height = 160,
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
            Topmost = true,
            Background = new SolidColorBrush(Color.Parse("#1A1A1A")),
            CanResize = false
        };

        var stackPanel = new StackPanel
        {
            Margin = new Thickness(20),
            Spacing = 12
        };

        var label = new TextBlock
        {
            Text = "Nhập Gemini API Key của bạn (sẽ được mã hoá bảo mật):",
            Foreground = Brushes.White,
            FontSize = 13,
            FontWeight = FontWeight.Medium
        };

        var textBox = new TextBox
        {
            PlaceholderText = "AIzaSy...",
            PasswordChar = '*',
            Text = _secretManager.GetSecret("GeminiApiKey") ?? "",
            Foreground = Brushes.White,
            Background = new SolidColorBrush(Color.Parse("#2D2D2D")),
            BorderBrush = new SolidColorBrush(Color.Parse("#3D3D3D")),
            Height = 32,
            VerticalContentAlignment = VerticalAlignment.Center
        };

        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 10
        };

        var btnSave = new Button
        {
            Content = "Lưu lại",
            Width = 90,
            Height = 30,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            Background = new SolidColorBrush(Color.Parse("#007ACC")),
            Foreground = Brushes.White,
            Cursor = new Cursor(StandardCursorType.Hand)
        };

        var btnCancel = new Button
        {
            Content = "Hủy",
            Width = 90,
            Height = 30,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            Background = new SolidColorBrush(Color.Parse("#3D3D3D")),
            Foreground = Brushes.White,
            Cursor = new Cursor(StandardCursorType.Hand)
        };

        btnSave.Click += (s, e) =>
        {
            var key = textBox.Text?.Trim();
            if (!string.IsNullOrEmpty(key))
            {
                _secretManager.StoreSecret("GeminiApiKey", key);
                _logger.LogInfo("[FloatingBubble] Đã cập nhật Gemini API Key mới vào Windows DPAPI.");
            }
            else
            {
                _secretManager.RemoveSecret("GeminiApiKey");
                _logger.LogInfo("[FloatingBubble] Đã xóa Gemini API Key khỏi Secret Manager.");
            }
            inputWindow.Close();
        };

        btnCancel.Click += (s, e) =>
        {
            inputWindow.Close();
        };

        buttonPanel.Children.Add(btnSave);
        buttonPanel.Children.Add(btnCancel);

        stackPanel.Children.Add(label);
        stackPanel.Children.Add(textBox);
        stackPanel.Children.Add(buttonPanel);

        inputWindow.Content = stackPanel;

        await inputWindow.ShowDialog(this);
    }
}
