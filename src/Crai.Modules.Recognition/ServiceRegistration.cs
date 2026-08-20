using Microsoft.Extensions.DependencyInjection;
using Crai.Application.Contracts.Services;
using Crai.Modules.Recognition.Services;

namespace Crai.Modules.Recognition;

public static class ServiceRegistration
{
    public static IServiceCollection AddRecognitionModuleServices(this IServiceCollection services)
    {
        services.AddSingleton<WindowsOcrService>();
        services.AddSingleton<AiOcrService>();
        services.AddSingleton<IRecognitionService, OcrRouter>();

        return services;
    }
}
