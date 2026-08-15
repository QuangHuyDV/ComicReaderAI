using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Crai.Application.Contracts.Services;
using Crai.Desktop.ViewModels;
using Crai.Desktop.Views;
using Crai.Desktop.Services;
using Crai.Desktop.Feasibility;
using System;

namespace Crai.Desktop;

public partial class App : Avalonia.Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // 1. Khởi tạo Dependency Injection Container (Composition Root)
        CompositionRoot.Initialize();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // 2. Resolve MainViewModel thông qua DI Container để tự động inject PipelineRuntime
            var mainViewModel = CompositionRoot.ServiceProvider.GetRequiredService<MainViewModel>();

            var mainWindow = new MainWindow
            {
                DataContext = mainViewModel,
            };
            desktop.MainWindow = mainWindow;

            // 3. Đăng ký Window động cho TargetWindowProvider để CaptureService có thể render window này
            var windowProvider = CompositionRoot.ServiceProvider.GetRequiredService<ITargetWindowProvider>() as TargetWindowProvider;
            if (windowProvider != null)
            {
                windowProvider.TargetWindow = mainWindow;
            }

            // 4. Log thông tin chẩn đoán DPI
            DpiDiagnostic.Log(mainWindow);
        }

        base.OnFrameworkInitializationCompleted();
    }
}