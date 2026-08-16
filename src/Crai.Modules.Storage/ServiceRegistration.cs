using Microsoft.Extensions.DependencyInjection;
using Crai.Application.Contracts.Services;
using Crai.Modules.Storage.Services;
using Crai.Application.Contracts.Infrastructure;

namespace Crai.Modules.Storage;

public static class ServiceRegistration
{
    public static IServiceCollection AddStorageModuleServices(this IServiceCollection services)
    {
        // Đăng ký SQLite Local Translation Cache làm Singleton
        services.AddSingleton<ITranslationCache>(sp => new SqliteTranslationCache(
            sp.GetRequiredService<IStructuredLogger>()
        ));

        return services;
    }
}
