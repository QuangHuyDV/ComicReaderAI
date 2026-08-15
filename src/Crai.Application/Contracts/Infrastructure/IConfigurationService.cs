using System;

namespace Crai.Application.Contracts.Infrastructure;

public interface IConfigurationService
{
    /// <summary>
    /// Lấy giá trị cấu hình theo key.
    /// </summary>
    T? GetValue<T>(string key);

    /// <summary>
    /// Lấy một section cấu hình và map trực tiếp sang class POCO.
    /// </summary>
    T GetSection<T>(string sectionName) where T : class, new();

    /// <summary>
    /// Ép buộc nạp lại cấu hình từ disk.
    /// </summary>
    void Reload();
}
