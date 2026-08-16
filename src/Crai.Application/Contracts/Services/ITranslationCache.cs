using System.Threading;
using System.Threading.Tasks;

namespace Crai.Application.Contracts.Services;

public interface ITranslationCache
{
    /// <summary>
    /// Lấy bản dịch đã lưu trong cache theo text gốc và ngôn ngữ đích.
    /// Trả về null nếu cache miss (không tìm thấy).
    /// </summary>
    Task<string?> GetAsync(string sourceText, string targetLanguage, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lưu bản dịch vào cache để tái sử dụng.
    /// </summary>
    Task SetAsync(string sourceText, string targetLanguage, string translatedText, CancellationToken cancellationToken = default);

    /// <summary>
    /// Xóa sạch cache cục bộ.
    /// </summary>
    Task ClearAsync(CancellationToken cancellationToken = default);
}
