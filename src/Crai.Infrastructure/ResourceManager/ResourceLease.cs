using System;
using Crai.Application.Contracts.Infrastructure;

namespace Crai.Infrastructure.ResourceManager;

public class ResourceLease<T> : IResourceLease<T> where T : class
{
    private readonly Action<IResourceLease<T>> _releaseAction;
    private bool _disposed;

    public string ResourceId { get; }
    public T Resource { get; }

    public ResourceLease(string resourceId, T resource, Action<IResourceLease<T>> releaseAction)
    {
        ResourceId = resourceId ?? throw new ArgumentNullException(nameof(resourceId));
        Resource = resource ?? throw new ArgumentNullException(nameof(resource));
        _releaseAction = releaseAction ?? throw new ArgumentNullException(nameof(releaseAction));
    }

    public void Dispose()
    {
        if (_disposed) return;
        _releaseAction(this);
        _disposed = true;
    }
}
