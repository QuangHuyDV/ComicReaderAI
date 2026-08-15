using System;

namespace Crai.Domain.Runtime;

public class WorkItem
{
    /// <summary>
    /// Định danh duy nhất cho WorkItem.
    /// </summary>
    public WorkItemId Id { get; }

    /// <summary>
    /// Thời điểm khởi tạo WorkItem.
    /// </summary>
    public DateTime CreatedAt { get; }

    /// <summary>
    /// Trạng thái hiện tại của WorkItem trong pipeline.
    /// </summary>
    public WorkItemStatus Status { get; set; }

    /// <summary>
    /// Đường dẫn file ảnh chụp màn hình nguồn (hoặc định danh buffer ảnh).
    /// </summary>
    public string? RawImagePath { get; set; }

    /// <summary>
    /// Kết quả nhận dạng văn bản thô (OCR Output).
    /// </summary>
    public string? RawText { get; set; }

    /// <summary>
    /// Kết quả dịch thuật sang ngôn ngữ đích (Translation Output).
    /// </summary>
    public string? TranslatedText { get; set; }

    /// <summary>
    /// Chi tiết thông báo lỗi (nếu Status là Failed).
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Thời điểm kết thúc xử lý hoàn toàn.
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    public WorkItem()
    {
        Id = WorkItemId.New();
        CreatedAt = DateTime.UtcNow;
        Status = WorkItemStatus.Created;
    }

    public void MarkAsFailed(string errorMessage)
    {
        Status = WorkItemStatus.Failed;
        ErrorMessage = errorMessage;
        CompletedAt = DateTime.UtcNow;
    }

    public void MarkAsCompleted(string translatedText)
    {
        Status = WorkItemStatus.Completed;
        TranslatedText = translatedText;
        CompletedAt = DateTime.UtcNow;
    }
}
