using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using System;
using System.Diagnostics;

namespace Crai.Desktop.Feasibility;

/// <summary>
/// Bước 0.2: DPI và multi-monitor diagnostic helper.
/// Gọi Log() trong App.OnStartup để in thông tin DPI ra console.
/// </summary>
public static class DpiDiagnostic
{
    public static void Log(Window window)
    {
        var screens = window.Screens;
        Console.WriteLine($"[DPI Diagnostic] Screen count: {screens.ScreenCount}");

        int index = 0;
        foreach (var screen in screens.All)
        {
            Console.WriteLine($"  Screen {index++}:");
            Console.WriteLine($"    WorkingArea : {screen.WorkingArea}");
            Console.WriteLine($"    Bounds      : {screen.Bounds}");
            Console.WriteLine($"    Scaling     : {screen.Scaling} ({screen.Scaling * 96:F0} DPI)");
            Console.WriteLine($"    IsPrimary   : {screen.IsPrimary}");
        }

        var sw = Stopwatch.StartNew();
        window.Opened += (_, _) =>
        {
            sw.Stop();
            Console.WriteLine($"[Startup] Window opened in {sw.ElapsedMilliseconds}ms");
        };
    }
}
