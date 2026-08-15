using Microsoft.Extensions.DependencyInjection;
using Crai.Application.Contracts.Services;
using Crai.Modules.Capture.Services;

namespace Crai.Modules.Capture;

public static class ServiceRegistration
{
    public static IServiceCollection AddCaptureModuleServices(this IServiceCollection services)
    {
        // Đăng ký dịch vụ Capture sử dụng Avalonia RenderTargetBitmap
        services.AddSingleton<ICaptureService, CaptureService>();

        return services;
    }
}
