using System;
using System.Threading;
using System.Threading.Tasks;
using Crai.Application.Contracts.Services;
using Crai.Application.Contracts.Infrastructure;

namespace Crai.Modules.Presentation.Services;

public class OverlayPresentationService : IPresentationService
{
    private readonly IStructuredLogger _logger;

    public string? LatestText { get; private set; }

    public OverlayPresentationService(IStructuredLogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task PresentTranslationAsync(string translatedText, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        LatestText = translatedText;
        _logger.LogInfo($"[PresentationService] Hiển thị bản dịch lên UI Overlay: \"{translatedText}\"");
        
        // Trong phiên bản đầy đủ, phần code này sẽ Dispatch lệnh vẽ TextBox/TextBlock 
        // lên lớp phủ Overlay của Avalonia Window.
        return Task.CompletedTask;
    }
}
