using System;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Win32;

namespace Crai.Desktop.Feasibility;

/// <summary>
/// Bước 0.4: Global Hotkey prototype dùng Win32 RegisterHotKey và Avalonia WndProc hook.
/// </summary>
public class GlobalHotkeyProto : IDisposable
{
    private const int WM_HOTKEY = 0x0312;
    private const int MOD_CONTROL = 0x0002;
    private const int MOD_SHIFT = 0x0004;
    private const int VK_T = 0x54; // Phím T
    private const int HOTKEY_ID = 9001;

    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    private readonly Window _window;
    private IntPtr _hwnd;
    private bool _registered;

    public event Action? HotkeyTriggered;

    public GlobalHotkeyProto(Window window)
    {
        _window = window;
        _window.Loaded += Window_Loaded;
    }

    private void Window_Loaded(object? sender, EventArgs e)
    {
        var platformHandle = _window.TryGetPlatformHandle();
        _hwnd = platformHandle?.Handle ?? IntPtr.Zero;
        if (_hwnd != IntPtr.Zero)
        {
            // Sử dụng Win32Properties của Avalonia để hook vào WndProc
            Win32Properties.AddWndProcHookCallback(_window, WndProcHook);
            Register();
        }
        else
        {
            Console.WriteLine("[Hotkey] Can't register, hWnd is null");
        }
    }

    private bool Register()
    {
        _registered = RegisterHotKey(_hwnd, HOTKEY_ID, MOD_CONTROL | MOD_SHIFT, VK_T);
        Console.WriteLine(_registered
            ? "[Hotkey] Ctrl+Shift+T registered OK"
            : "[Hotkey] FAILED to register — check if another app has this hotkey");
        return _registered;
    }

    private IntPtr WndProcHook(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_HOTKEY && wParam.ToInt32() == HOTKEY_ID)
        {
            Console.WriteLine("[Hotkey] Ctrl+Shift+T triggered!");
            HotkeyTriggered?.Invoke();
            handled = true; // Đánh dấu đã xử lý
        }
        return IntPtr.Zero;
    }

    public void Dispose()
    {
        if (_registered && _hwnd != IntPtr.Zero)
        {
            UnregisterHotKey(_hwnd, HOTKEY_ID);
            _registered = false;
            Win32Properties.RemoveWndProcHookCallback(_window, WndProcHook);
            Console.WriteLine("[Hotkey] Unregistered and hook removed");
        }
    }
}
