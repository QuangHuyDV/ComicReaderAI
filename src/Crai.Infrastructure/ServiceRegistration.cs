using Microsoft.Extensions.DependencyInjection;
using Crai.Application.Contracts.Infrastructure;
using Crai.Infrastructure.Configuration;
using Crai.Infrastructure.Logging;
using Crai.Infrastructure.EventBus;
using Crai.Infrastructure.Telemetry;
using Crai.Infrastructure.Scheduler;
using Crai.Infrastructure.ResourceManager;
using Crai.Infrastructure.Secret;

namespace Crai.Infrastructure;

public static class ServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        // Đăng ký central Configuration Service
        services.AddSingleton<IConfigurationService>(sp => new ConfigurationService());

        // Đăng ký structured Logger
        services.AddSingleton<IStructuredLogger, StructuredLogger>();

        // Đăng ký Event Bus in-process
        services.AddSingleton<IEventBus, InMemoryEventBus>();

        // Đăng ký Telemetry Service
        services.AddSingleton<ITelemetryService, InMemoryTelemetryService>();

        // Đăng ký Background Task Scheduler
        services.AddSingleton<IScheduler, InMemoryScheduler>();

        // Đăng ký Resource Manager
        services.AddSingleton<IResourceManager, InMemoryResourceManager>();

        // Đăng ký Windows DPAPI Secret Manager
        services.AddSingleton<ISecretManager>(sp => new DpapiSecretManager(sp.GetRequiredService<IStructuredLogger>()));

        return services;
    }
}
