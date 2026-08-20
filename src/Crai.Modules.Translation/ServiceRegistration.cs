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
            sp.GetRequiredService<IConfigurationService>(),
            sp.GetRequiredService<IStructuredLogger>()
        ));

        // 2. Đăng ký Central Translation Service thông qua Router để hỗ trợ fallback và caching
        services.AddSingleton<ITranslationService, TranslationRouter>(sp => new TranslationRouter(
            sp.GetRequiredService<GoogleTranslationEngine>(),
            sp.GetRequiredService<GeminiTranslationEngine>(),
            sp.GetRequiredService<IConfigurationService>(),
            sp.GetRequiredService<ITranslationCache>(),
            sp.GetRequiredService<IStructuredLogger>()
        ));

        return services;
    }
}
