using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Crai.Desktop.ViewModels;
using Crai.Desktop.Views;
using Crai.Desktop.Feasibility;
using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;

namespace Crai.Desktop;

public partial class App : Application
{
    private GlobalHotkeyProto? _hotkeyProto;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindow = new MainWindow
            {
                DataContext = new MainViewModel(),
            };
            desktop.MainWindow = mainWindow;

            // Bước 0.2: DPI diagnostic
            DpiDiagnostic.Log(mainWindow);

            // Bước 0.3: Side Panel Prototype
            var sidePanel = new SidePanelProto();
            sidePanel.Show();

            // Bước 0.4: Global Hotkey
            _hotkeyProto = new GlobalHotkeyProto(mainWindow);
            _hotkeyProto.HotkeyTriggered += () =>
            {
                Console.WriteLine("[App] Global Hotkey (Ctrl+Shift+T) Action triggered!");
            };

            // Bước 0.5: RenderTargetBitmap Capture Test (Bypass OS restriction)
            mainWindow.Loaded += async (sender, e) =>
            {
                await Task.Delay(1500); // Đợi 1.5s cho window render hoàn toàn và hiển thị
                try
                {
                    Console.WriteLine("[App] Starting RenderTargetBitmap capture test...");
                    string outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "capture_test.png");
                    
                    if (File.Exists(outputPath)) File.Delete(outputPath);

                    // Lấy bounds và scaling
                    var bounds = mainWindow.Bounds;
                    var scaling = mainWindow.Screens.Primary?.Scaling ?? 1.0;
                    
                    // Tạo size vật lý dựa trên DPI scaling
                    var pixelSize = new PixelSize(
                        (int)(bounds.Width * scaling),
                        (int)(bounds.Height * scaling)
                    );
                    
                    var dpi = new Vector(96 * scaling, 96 * scaling);

                    using (var bitmap = new RenderTargetBitmap(pixelSize, dpi))
                    {
                        bitmap.Render(mainWindow);
                        bitmap.Save(outputPath);
                    }

                    if (File.Exists(outputPath))
                    {
                        var fileInfo = new FileInfo(outputPath);
                        Console.WriteLine($"[App] Capture SUCCESS! File size: {fileInfo.Length} bytes. Path: {outputPath}");
                    }
                    else
                    {
                        Console.WriteLine("[App] Capture FAILED: Output file was not created");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[App] Capture EXCEPTION: {ex.Message}\n{ex.StackTrace}");
                }
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}