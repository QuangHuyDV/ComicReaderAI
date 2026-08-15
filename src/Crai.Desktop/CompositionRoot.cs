using System;
using Microsoft.Extensions.DependencyInjection;
using Crai.Application.Contracts.Services;
using Crai.Infrastructure;
using Crai.Platform.Windows;
using Crai.Modules.Capture;
using Crai.Modules.Recognition;
using Crai.Modules.Translation;
using Crai.Modules.Presentation;
using Crai.Modules.TextProcessing;
using Crai.Runtime;
using Crai.Desktop.Services;
using Crai.Desktop.ViewModels;

namespace Crai.Desktop;

public static class CompositionRoot
{
    private static IServiceProvider? _serviceProvider;

    public static IServiceProvider ServiceProvider
    {
        get
        {
            if (_serviceProvider == null)
            {
                throw new InvalidOperationException("CompositionRoot has not been initialized. Call Initialize() first.");
            }
            return _serviceProvider;
        }
    }

    public static void Initialize()
    {
        var services = new ServiceCollection();

        // 1. Đăng ký UI Window Provider động làm Singleton
        services.AddSingleton<ITargetWindowProvider, TargetWindowProvider>();

        // 2. Đăng ký các dịch vụ cốt lõi và các platform-specific adapters
        services.AddInfrastructureServices();
        services.AddWindowsPlatformServices();
        services.AddRuntimeServices();

        // 3. Đăng ký các module nghiệp vụ của Modular Monolith
        services.AddCaptureModuleServices();
        services.AddRecognitionModuleServices();
        services.AddTranslationModuleServices();
        services.AddPresentationModuleServices();
        services.AddTextProcessingModuleServices();

        // 4. Đăng ký các ViewModels của Desktop App (để tự động resolve dependencies qua DI)
        services.AddTransient<MainViewModel>();

        _serviceProvider = services.BuildServiceProvider();
    }
}
