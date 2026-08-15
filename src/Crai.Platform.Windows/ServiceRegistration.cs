using Microsoft.Extensions.DependencyInjection;

namespace Crai.Platform.Windows;

public static class ServiceRegistration
{
    public static IServiceCollection AddWindowsPlatformServices(this IServiceCollection services)
    {
        // Đăng ký các dịch vụ chạy native trên hệ điều hành Windows (WinRT OCR, Windows Hotkey)
        return services;
    }
}
