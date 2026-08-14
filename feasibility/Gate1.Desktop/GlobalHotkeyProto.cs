using System;
using System.Runtime.InteropServices;
using System.Windows.Forms; // chỉ dùng cho message loop nếu cần
using Avalonia;
using Avalonia.Controls;

namespace Crai.Desktop.Feasibility;

/// <summary>
/// Bước 0.4: Global Hotkey prototype dùng Win32 RegisterHotKey.
///
/// Cách dùng trong App.OnStartup:
///   _hotkeyProto = new GlobalHotkeyProto(mainWindow.TryGetPlatformHandle()!.Handle);
///   _hotkeyProto.Register();
///
/// NOTE: Avalonia không expose message pump trực tiếp.
/// Cần dùng Win32Interop để hook WndProc — xem README trong file này.
/// </summary>
public class GlobalHotkeyProto : IDisposable
{
    // Win32 constants
    private const int WM_HOTKEY = 0x0312;
    private const int MOD_CONTROL = 0x0002;
    private const int MOD_SHIFT = 0x0004;
    private const int VK_T = 0x54;
    private const int HOTKEY_ID = 9001;

    [DllImport("user32.dll")] private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
    [DllImport("user32.dll")] private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    private readonly IntPtr _hwnd;
    private bool _registered;

    public event Action? HotkeyTriggered;

    public GlobalHotkeyProto(IntPtr hwnd)
    {
        _hwnd = hwnd;
    }

    /// <summary>
    /// Đăng ký Ctrl+Shift+T
    /// </summary>
    public bool Register()
    {
        _registered = RegisterHotKey(_hwnd, HOTKEY_ID, MOD_CONTROL | MOD_SHIFT, VK_T);
        Console.WriteLine(_registered
            ? "[Hotkey] Ctrl+Shift+T registered OK"
            : "[Hotkey] FAILED to register — check if another app has this hotkey");
        return _registered;
    }

    /// <summary>
    /// Gọi từ WndProc khi nhận WM_HOTKEY message.
    /// Tích hợp với Avalonia Win32 message interop.
    /// </summary>
    public bool HandleMessage(IntPtr msg, IntPtr wParam)
    {
        if (msg == WM_HOTKEY && wParam.ToInt32() == HOTKEY_ID)
        {
            Console.WriteLine("[Hotkey] Ctrl+Shift+T triggered!");
            HotkeyTriggered?.Invoke();
            return true;
        }
        return false;
    }

    public void Dispose()
    {
        if (_registered)
        {
            UnregisterHotKey(_hwnd, HOTKEY_ID);
            _registered = false;
            Console.WriteLine("[Hotkey] Unregistered");
        }
    }
}

/*
README — Avalonia Win32 WndProc Integration:

Avalonia sử dụng native Win32 window.
Để nhận WM_HOTKEY:
1. Lấy HWND: window.TryGetPlatformHandle()?.Handle
2. Subclass window với SetWindowSubclass (Win32 API) hoặc
   dùng Avalonia IPlatformHandle interop

Xem: https://github.com/AvaloniaUI/Avalonia/discussions
Tag: Win32Interop, WndProc, Subclass

Alternative nếu WndProc quá phức tạp:
- Dùng thư viện: GlobalKeyboardHook (NuGet)
  - Package: 'SharpHook' hoặc 'NHotkey.Wpf' port
  - Test với Avalonia compatibility
*/
