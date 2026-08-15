namespace Crai.Application.Contracts.Services;

public interface ITextProcessorService
{
    /// <summary>
    /// Làm sạch, định dạng và chuẩn hóa văn bản quét được từ OCR (loại bỏ dòng thừa, dấu cách lạ, ký tự rác).
    /// </summary>
    string NormalizeText(string rawText);
}
