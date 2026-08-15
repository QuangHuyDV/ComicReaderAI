using Microsoft.Extensions.DependencyInjection;
using Crai.Application.Contracts.Services;
using Crai.Modules.TextProcessing.Services;

namespace Crai.Modules.TextProcessing;

public static class ServiceRegistration
{
    public static IServiceCollection AddTextProcessingModuleServices(this IServiceCollection services)
    {
        // Đăng ký dịch vụ làm sạch và xử lý văn bản
        services.AddSingleton<ITextProcessorService, TextProcessorService>();

        return services;
    }
}
