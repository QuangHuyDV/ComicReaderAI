using System;

namespace Crai.Application.Contracts.Infrastructure;

public interface IScheduler
{
    /// <summary>
    /// Đăng ký và khởi chạy một background task theo cấu hình định sẵn.
    /// </summary>
    void RegisterTask(TaskDefinition definition);

    /// <summary>
    /// Buộc thực thi task có Id tương ứng ngay lập tức (bất kể interval).
    /// </summary>
    void TriggerNow(string taskId);

    /// <summary>
    /// Hủy đăng ký và dừng một task đang chạy.
    /// </summary>
    void CancelTask(string taskId);

    /// <summary>
    /// Dừng toàn bộ các background tasks đang chạy trong Scheduler một cách an toàn.
    /// </summary>
    void Shutdown();
}
