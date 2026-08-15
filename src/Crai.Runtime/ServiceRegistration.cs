using Microsoft.Extensions.DependencyInjection;
using Crai.Application.Contracts.Runtime;
using Crai.Runtime.Storage;
using Crai.Runtime.Engine;

namespace Crai.Runtime;

public static class ServiceRegistration
{
    public static IServiceCollection AddRuntimeServices(this IServiceCollection services)
    {
        // Đăng ký Artifact Store (in-memory)
        services.AddSingleton<IArtifactStore, InMemoryArtifactStore>();

        // Đăng ký Pipeline Execution Engine Runtime
        services.AddSingleton<IPipelineRuntime, PipelineRuntime>();

        return services;
    }
}
