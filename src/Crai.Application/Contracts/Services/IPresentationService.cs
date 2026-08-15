using System.Threading;
using System.Threading.Tasks;

namespace Crai.Application.Contracts.Services;

public interface IPresentationService
{
    /// <summary>
    /// Hiển thị bản dịch lên lớp phủ Overlay hoặc Side Panel UI cho người dùng xem.
    /// </summary>
    Task PresentTranslationAsync(string translatedText, CancellationToken cancellationToken = default);
}
