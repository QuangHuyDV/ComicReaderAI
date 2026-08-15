using System.Threading;
using System.Threading.Tasks;

namespace Crai.Application.Contracts.Services;

public interface ITranslationService
{
    /// <summary>
    /// Thực hiện dịch thuật chuỗi văn bản thô sang ngôn ngữ đích (Tiếng Việt) và trả về kết quả dịch.
    /// </summary>
    Task<string> TranslateTextAsync(string rawText, CancellationToken cancellationToken = default);
}
