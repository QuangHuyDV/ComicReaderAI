using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Crai.Desktop.Feasibility;
using Crai.Desktop.ViewModels;

namespace Crai.Desktop.Views;

public partial class MainWindow : Window
{
    private GlobalHotkeyProto? _hotkeyProto;

    public MainWindow()
    {
        InitializeComponent();

        // Tự động Dock vào cạnh phải màn hình khi mở
        Opened += MainWindow_Opened;

        // Đăng ký Global Hotkey Ctrl+Shift+T
        _hotkeyProto = new GlobalHotkeyProto(this);
        _hotkeyProto.HotkeyTriggered += OnHotkeyTriggered;

        // Dọn dẹp hotkey khi đóng window
        Closed += MainWindow_Closed;
    }

    private void MainWindow_Opened(object? sender, EventArgs e)
    {
        DockToRight();
    }

    private void MainWindow_Closed(object? sender, EventArgs e)
    {
        _hotkeyProto?.Dispose();
        _hotkeyProto = null;
    }

    private async void OnHotkeyTriggered()
    {
        if (DataContext is MainViewModel vm)
        {
            // Trigger lệnh dịch từ ViewModel
            await vm.TriggerTranslationAsync();
        }
    }

    public void CloseButton_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void DockToRight()
    {
        var screen = Screens.Primary;
        if (screen is null) return;

        var wa = screen.WorkingArea;
        double scaling = screen.Scaling;

        // Định vị trí Side Panel sát cạnh phải của màn hình chính
        Position = new PixelPoint(
            (int)(wa.X + wa.Width - (Width * scaling)),
            wa.Y
        );
        Height = wa.Height / scaling;
    }
}