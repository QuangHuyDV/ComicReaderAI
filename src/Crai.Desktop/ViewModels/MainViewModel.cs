using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Crai.Application.Contracts.Runtime;
using Crai.Domain.Runtime;

namespace Crai.Desktop.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly IPipelineRuntime _pipelineRuntime;

    [ObservableProperty]
    private string _statusMessage = "Đang chờ phím tắt (Ctrl+Shift+T)...";

    [ObservableProperty]
    private string _originalText = "Nhấn Hotkey để chụp màn hình và bắt đầu quét...";

    [ObservableProperty]
    private string _translatedText = "Bản dịch tiếng Việt sẽ xuất hiện tại đây.";

    [ObservableProperty]
    private bool _isLoading = false;

    public MainViewModel(IPipelineRuntime pipelineRuntime)
    {
        _pipelineRuntime = pipelineRuntime ?? throw new ArgumentNullException(nameof(pipelineRuntime));
        
        // Đăng ký sự kiện cập nhật trạng thái từ Runtime
        _pipelineRuntime.WorkItemUpdated += OnWorkItemUpdated;
    }

    [RelayCommand]
    public async Task TriggerTranslationAsync()
    {
        if (IsLoading) return;

        IsLoading = true;
        StatusMessage = "Đang bắt đầu dịch...";

        try
        {
            await _pipelineRuntime.TriggerExecutionAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Lỗi: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void OnWorkItemUpdated(WorkItem item)
    {
        // Cập nhật trạng thái hiển thị dựa trên status của WorkItem
        StatusMessage = item.Status switch
        {
            WorkItemStatus.Created => "Đã khởi tạo...",
            WorkItemStatus.Capturing => "Đang chụp màn hình...",
            WorkItemStatus.Captured => "Đã chụp màn hình thành công.",
            WorkItemStatus.Recognizing => "Đang nhận diện chữ (OCR)...",
            WorkItemStatus.Recognized => "Đã nhận diện chữ thành công.",
            WorkItemStatus.Translating => "Đang dịch thuật...",
            WorkItemStatus.Translated => "Đã dịch thuật thành công.",
            WorkItemStatus.Presenting => "Đang kết xuất lên UI...",
            WorkItemStatus.Completed => "Hoàn thành!",
            WorkItemStatus.Failed => $"Thất bại: {item.ErrorMessage}",
            _ => StatusMessage
        };

        // Cập nhật dữ liệu văn bản nếu có thay đổi
        if (!string.IsNullOrWhiteSpace(item.RawText))
        {
            OriginalText = item.RawText;
        }

        if (!string.IsNullOrWhiteSpace(item.TranslatedText))
        {
            TranslatedText = item.TranslatedText;
        }
    }
}
