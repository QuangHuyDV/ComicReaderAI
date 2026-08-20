using System;
using System.Threading;
using System.Threading.Tasks;
using Crai.Application.Contracts.Services;
using Crai.Application.Contracts.Infrastructure;

namespace Crai.Modules.Recognition.Services;

public class OcrRouter : IRecognitionService
{
    private readonly WindowsOcrService _windowsOcr;
    private readonly AiOcrService _aiOcr;
    private readonly IConfigurationService _configService;

    public OcrRouter(WindowsOcrService windowsOcr, AiOcrService aiOcr, IConfigurationService configService)
    {
        _windowsOcr = windowsOcr ?? throw new ArgumentNullException(nameof(windowsOcr));
        _aiOcr = aiOcr ?? throw new ArgumentNullException(nameof(aiOcr));
        _configService = configService ?? throw new ArgumentNullException(nameof(configService));
    }

    public async Task<OcrResultInfo> RecognizeTextAsync(string imagePath, CancellationToken cancellationToken = default)
    {
        var engine = _configService.GetValue<string>("OCR:Engine") ?? "Windows";
        if (engine.Equals("AI", StringComparison.OrdinalIgnoreCase))
        {
            return await _aiOcr.RecognizeTextAsync(imagePath, cancellationToken);
        }
        return await _windowsOcr.RecognizeTextAsync(imagePath, cancellationToken);
    }
}
