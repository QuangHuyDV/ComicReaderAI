using System;
using System.Collections.Generic;
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
        OverlayCanvas.Children.Clear();

        // Lấy thông số DPI Scaling của màn hình chính để sửa lỗi lệch tọa độ
        var screen = Screens.Primary ?? Screens.All[0];
        double scaling = screen.Scaling;

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
            // - Nền đen đặc hoàn toàn (Opacity 100%) che chữ gốc phía sau tuyệt đối.
            // - Không dùng viền màu chói mắt, dùng viền mờ tối giản trùng màu nền.
            // - Padding khít giúp gọn gàng, tránh làm mất diện tích game/truyện.
            var border = new Border
            {
                Background = new SolidColorBrush(Color.Parse("#FF181818")), // Đen đặc che chữ gốc 100%
                BorderBrush = new SolidColorBrush(Color.Parse("#FF2A2A2A")),  // Viền tối giản mờ trùng màu nền
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

    private void OnBackgroundClicked(object? sender, PointerPressedEventArgs e)
    {
        _autoDismissTimer.Stop();
        Close();
    }

    private void CloseButton_Click(object? sender, RoutedEventArgs e)
    {
        _autoDismissTimer.Stop();
        Close();
    }
}
