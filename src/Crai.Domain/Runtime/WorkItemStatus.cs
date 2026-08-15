namespace Crai.Domain.Runtime;

public enum WorkItemStatus
{
    /// <summary>
    /// WorkItem mới được khởi tạo.
    /// </summary>
    Created,

    /// <summary>
    /// Đang trong quá trình chụp màn hình nguồn.
    /// </summary>
    Capturing,

    /// <summary>
    /// Đã chụp màn hình thành công.
    /// </summary>
    Captured,

    /// <summary>
    /// Đang trong quá trình nhận dạng chữ (OCR).
    /// </summary>
    Recognizing,

    /// <summary>
    /// Đã nhận dạng chữ thành công.
    /// </summary>
    Recognized,

    /// <summary>
    /// Đang trong quá trình dịch thuật.
    /// </summary>
    Translating,

    /// <summary>
    /// Đã dịch thuật thành công.
    /// </summary>
    Translated,

    /// <summary>
    /// Đang trong quá trình hiển thị kết quả (render/presentation).
    /// </summary>
    Presenting,

    /// <summary>
    /// Đã hiển thị kết quả thành công (luồng hoàn tất).
    /// </summary>
    Completed,

    /// <summary>
    /// Xảy ra lỗi tại một trong các bước của pipeline.
    /// </summary>
    Failed
}
