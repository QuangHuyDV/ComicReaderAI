using Avalonia.Controls;

namespace Crai.Desktop.Feasibility;

/// <summary>
/// Bước 0.3: Side Panel window prototype.
/// Window stay-on-top, có thể dock vào cạnh phải màn hình.
/// Paste class này vào project, rồi thêm vào App.OnStartup:
///   new SidePanelProto().Show();
/// </summary>
public class SidePanelProto : Window
{
    public SidePanelProto()
    {
        Title = "CRAI Side Panel (Prototype)";
        Width = 400;
        Height = 700;
        CanResize = true;
        Topmost = true;           // Stay on top
        SystemDecorations = SystemDecorations.BorderOnly;

        // Dock to right edge of primary screen
        Opened += (_, _) => DockToRight();

        Content = new TextBlock
        {
            Text = "Side Panel — Step 0.3\n\n" +
                   "✓ Topmost = true\n" +
                   "✓ Dock to right edge\n" +
                   "✓ Resize test: drag edges\n\n" +
                   "Check: minimizing host app\n" +
                   "should NOT hide this panel.",
            Margin = new Avalonia.Thickness(16),
            FontSize = 14,
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
        };
    }

    private void DockToRight()
    {
        var screen = Screens.Primary;
        if (screen is null) return;

        var wa = screen.WorkingArea;
        double scaling = screen.Scaling;

        // Position window at right edge of working area
        Position = new Avalonia.PixelPoint(
            (int)(wa.X + wa.Width - (Width * scaling)),
            wa.Y
        );
        Height = wa.Height / scaling;
    }
}
