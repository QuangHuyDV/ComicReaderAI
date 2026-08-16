using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Crai.Application.Contracts.Services;

public class OcrLineInfo
{
    public string Text { get; set; } = string.Empty;
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public string TranslatedText { get; set; } = string.Empty;

    public OcrLineInfo() { }

    public OcrLineInfo(string text, double x, double y, double width, double height)
    {
        Text = text;
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }
}

public class OcrResultInfo
{
    public string FullText { get; set; } = string.Empty;
    public List<OcrLineInfo> Lines { get; set; } = new();

    public OcrResultInfo() { }

    public OcrResultInfo(string fullText, List<OcrLineInfo> lines)
    {
        FullText = fullText;
        Lines = lines;
    }
}

public interface IRecognitionService
{
    /// <summary>
    /// Thực hiện nhận diện chữ (OCR) từ file ảnh nguồn và trả về kết quả chi tiết kèm tọa độ các dòng.
    /// </summary>
    Task<OcrResultInfo> RecognizeTextAsync(string imagePath, CancellationToken cancellationToken = default);
}
