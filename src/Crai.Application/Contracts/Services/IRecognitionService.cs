using System.Threading;
using System.Threading.Tasks;

namespace Crai.Application.Contracts.Services;

public interface IRecognitionService
{
    /// <summary>
    /// Thực hiện nhận diện chữ (OCR) từ file ảnh nguồn và trả về chuỗi văn bản thô.
    /// </summary>
    Task<string> RecognizeTextAsync(string imagePath, CancellationToken cancellationToken = default);
}
