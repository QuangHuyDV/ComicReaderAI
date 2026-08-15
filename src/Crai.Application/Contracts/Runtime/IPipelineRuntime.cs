using System;
using System.Threading;
using System.Threading.Tasks;
using Crai.Domain.Runtime;

namespace Crai.Application.Contracts.Runtime;

public interface IPipelineRuntime
{
    /// <summary>
    /// Sự kiện phát sinh mỗi khi một WorkItem cập nhật trạng thái mới (chụp xong, ocr xong, lỗi, v.v.).
    /// </summary>
    event Action<WorkItem>? WorkItemUpdated;

    /// <summary>
    /// Kích hoạt một chu kỳ thực thi pipeline mới bất đồng bộ (Capture -> OCR -> Dịch -> Hiển thị).
    /// </summary>
    Task<WorkItem> TriggerExecutionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Dừng các hoạt động xử lý hiện tại của Runtime.
    /// </summary>
    void Stop();
}
