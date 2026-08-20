using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Crai.Application.Contracts.Services;

namespace Crai.Desktop.Views;

public partial class TranslationOverlayWindow : Window
{
    private readonly DispatcherTimer _autoDismissTimer;

    public TranslationOverlayWindow(int durationSeconds)
    {
        InitializeComponent();

        _autoDismissTimer = new DispatcherTimer();

        if (durationSeconds > 0)
        {
            _autoDismissTimer.Interval = TimeSpan.FromSeconds(durationSeconds);
            _autoDismissTimer.Tick += (s, e) =>
            {
                _autoDismissTimer.Stop();
                Close();
            };
            _autoDismissTimer.Start();
        }
    }

    public TranslationOverlayWindow() : this(8)
    {
    }

    public void RenderTranslations(List<OcrLineInfo> lines)
    {
        RenderTranslations(lines, false);
    }

    public void RenderTranslations(List<OcrLineInfo> lines, bool isTranslucent)
    {
        OverlayCanvas.Children.Clear();

        // Lấy thông số DPI Scaling của màn hình chính để sửa lỗi lệch tọa độ
        var screen = Screens.Primary ?? Screens.All[0];
        double scaling = screen.Scaling;

        // Chọn màu nền và viền dựa trên độ mờ mong muốn
        var bgBrush = isTranslucent
            ? new SolidColorBrush(Color.Parse("#A6121212")) // ~65% opacity để nhìn xuyên nền
            : new SolidColorBrush(Color.Parse("#FF181818")); // 100% opacity đen đặc che hoàn toàn

        var borderBrush = isTranslucent
            ? new SolidColorBrush(Color.Parse("#882A2A2A")) // Viền mờ hơn
            : new SolidColorBrush(Color.Parse("#FF2A2A2A"));

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line.Text)) continue;

            var displayText = !string.IsNullOrWhiteSpace(line.TranslatedText) 
                ? line.TranslatedText 
                : "[...]";

            // Sửa lỗi lệch tọa độ: Chuyển đổi tọa độ pixel vật lý (OCR) sang tọa độ logic (Avalonia)
            double logicalX = line.X / scaling;
            double logicalY = line.Y / scaling;
            double logicalWidth = line.Width / scaling;
            double logicalHeight = line.Height / scaling;

            // Thiết kế nhãn dịch tự nhiên:
            // - Nền đen đặc hoặc mờ để che chữ gốc
            // - Không dùng viền màu chói mắt, dùng viền mờ tối giản trùng màu nền.
            // - Padding khít giúp gọn gàng, tránh làm mất diện tích game/truyện.
            var border = new Border
            {
                Background = bgBrush,
                BorderBrush = borderBrush,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(6, 3),
            };

            var textBlock = new TextBlock
            {
                Text = displayText,
                Foreground = new SolidColorBrush(Color.Parse("#FFE0E0E0")), // Chữ màu kem sáng dịu mắt, dễ đọc
                FontSize = 12,
                FontWeight = FontWeight.Normal,
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 16,
                TextAlignment = TextAlignment.Center
            };

            border.Child = textBlock;

            // Đặt tọa độ đè chính xác tuyệt đối lên chữ gốc
            Canvas.SetLeft(border, logicalX);
            Canvas.SetTop(border, logicalY);

            // Gán chiều rộng tương đương hoặc tối thiểu để gói chữ vừa vặn
            border.Width = Math.Max(logicalWidth, 80);
            
            OverlayCanvas.Children.Add(border);
        }
    }
    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);

        var platformHandle = TryGetPlatformHandle();
        var hwnd = platformHandle?.Handle ?? IntPtr.Zero;
        if (hwnd != IntPtr.Zero)
        {
            try
            {
                int exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
                SetWindowLong(hwnd, GWL_EXSTYLE, exStyle | WS_EX_TRANSPARENT | WS_EX_LAYERED);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Overlay] Lỗi thiết lập click-through: {ex.Message}");
            }
        }
    }

    [DllImport("user32.dll", EntryPoint = "GetWindowLong", SetLastError = true)]
    private static extern int GetWindowLong32(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr", SetLastError = true)]
    private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
    private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
    private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    private static int GetWindowLong(IntPtr hWnd, int nIndex)
    {
        if (IntPtr.Size == 8)
            return (int)GetWindowLongPtr64(hWnd, nIndex).ToInt64();
        else
            return GetWindowLong32(hWnd, nIndex);
    }

    private static void SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong)
    {
        if (IntPtr.Size == 8)
            SetWindowLongPtr64(hWnd, nIndex, new IntPtr(dwNewLong));
        else
            SetWindowLong32(hWnd, nIndex, dwNewLong);
    }

    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_TRANSPARENT = 0x00000020;
    private const int WS_EX_LAYERED = 0x00080000;
}
