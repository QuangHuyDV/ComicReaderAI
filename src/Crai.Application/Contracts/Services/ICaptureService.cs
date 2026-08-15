using System.Threading;
using System.Threading.Tasks;

namespace Crai.Application.Contracts.Services;

public interface ICaptureService
{
    /// <summary>
    /// Thực hiện chụp ảnh cửa sổ đích (Avalonia window hoặc target handle) 
    /// và lưu trữ ra đường dẫn chỉ định. Trả về đường dẫn ảnh nguồn thực tế.
    /// </summary>
    Task<string> CaptureTargetWindowAsync(string outputFilePath, CancellationToken cancellationToken = default);
}
