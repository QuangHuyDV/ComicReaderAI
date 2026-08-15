using Microsoft.Extensions.DependencyInjection;
using Crai.Application.Contracts.Services;
using Crai.Modules.Recognition.Services;

namespace Crai.Modules.Recognition;

public static class ServiceRegistration
{
    public static IServiceCollection AddRecognitionModuleServices(this IServiceCollection services)
    {
        // Đăng ký dịch vụ quét chữ Windows Media OCR (WinRT)
        services.AddSingleton<IRecognitionService, WindowsOcrService>();

        return services;
    }
}
