using System;
using System.Text.RegularExpressions;
using Crai.Application.Contracts.Services;

namespace Crai.Modules.TextProcessing.Services;

public class TextProcessorService : ITextProcessorService
{
    public string NormalizeText(string rawText)
    {
        if (string.IsNullOrWhiteSpace(rawText))
        {
            return string.Empty;
        }

        // 1. Loại bỏ các dòng trống thừa thãi hoặc các ký tự điều khiển lạ
        var cleaned = rawText.Replace("\r", "").Replace("\n", " ");

        // 2. Thay thế nhiều dấu cách liên tiếp bằng một dấu cách duy nhất
        cleaned = Regex.Replace(cleaned, @"\s+", " ");

        // 3. Trim khoảng trắng đầu/cuối
        return cleaned.Trim();
    }
}
