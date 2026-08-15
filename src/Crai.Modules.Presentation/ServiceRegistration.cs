using Microsoft.Extensions.DependencyInjection;
using Crai.Application.Contracts.Services;
using Crai.Modules.Presentation.Services;

namespace Crai.Modules.Presentation;

public static class ServiceRegistration
{
    public static IServiceCollection AddPresentationModuleServices(this IServiceCollection services)
    {
        // Đăng ký dịch vụ hiển thị bản dịch lên UI Overlay
        services.AddSingleton<IPresentationService, OverlayPresentationService>();

        return services;
    }
}
