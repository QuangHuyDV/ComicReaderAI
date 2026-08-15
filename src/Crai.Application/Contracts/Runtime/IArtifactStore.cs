using System.Collections.Generic;
using Crai.Domain.Runtime;

namespace Crai.Application.Contracts.Runtime;

public interface IArtifactStore
{
    /// <summary>
    /// Lưu trữ hoặc cập nhật thông tin một WorkItem.
    /// </summary>
    void SaveWorkItem(WorkItem item);

    /// <summary>
    /// Truy xuất WorkItem theo Id.
    /// </summary>
    WorkItem? GetWorkItem(WorkItemId id);

    /// <summary>
    /// Lấy danh sách các WorkItems gần nhất.
    /// </summary>
    IReadOnlyList<WorkItem> GetRecentWorkItems(int limit = 10);

    /// <summary>
    /// Xóa toàn bộ dữ liệu lịch sử trong store.
    /// </summary>
    void Clear();
}
