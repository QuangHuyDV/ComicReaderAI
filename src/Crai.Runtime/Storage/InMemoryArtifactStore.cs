using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Crai.Application.Contracts.Runtime;
using Crai.Domain.Runtime;

namespace Crai.Runtime.Storage;

public class InMemoryArtifactStore : IArtifactStore
{
    private readonly ConcurrentDictionary<WorkItemId, WorkItem> _store = new();

    public void SaveWorkItem(WorkItem item)
    {
        _store[item.Id] = item;
    }

    public WorkItem? GetWorkItem(WorkItemId id)
    {
        _store.TryGetValue(id, out var item);
        return item;
    }

    public IReadOnlyList<WorkItem> GetRecentWorkItems(int limit = 10)
    {
        return _store.Values
            .OrderByDescending(x => x.CreatedAt)
            .Take(limit)
            .ToList();
    }

    public void Clear()
    {
        _store.Clear();
    }
}
