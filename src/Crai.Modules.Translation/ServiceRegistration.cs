using Microsoft.Extensions.DependencyInjection;
using Crai.Application.Contracts.Services;
using Crai.Modules.Translation.Services;
using Crai.Application.Contracts.Infrastructure;

namespace Crai.Modules.Translation;

public static class ServiceRegistration
{
    public static IServiceCollection AddTranslationModuleServices(this IServiceCollection services)
    {
        // 1. Đăng ký các Concrete Engines phụ trợ
        services.AddSingleton<GoogleTranslationEngine>();
        
        services.AddSingleton<GeminiTranslationEngine>(sp => new GeminiTranslationEngine(
            sp.GetRequiredService<ISecretManager>(),
            sp.GetRequiredService<IStructuredLogger>()
        ));

        // 2. Đăng ký Central Translation Service thông qua Router để hỗ trợ fallback
        services.AddSingleton<ITranslationService, TranslationRouter>();

        return services;
    }
}
