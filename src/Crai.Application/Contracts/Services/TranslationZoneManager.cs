using System.Collections.Generic;

namespace Crai.Application.Contracts.Services;

public class TranslationZone
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }

    public TranslationZone() { }

    public TranslationZone(double x, double y, double width, double height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public bool Contains(double px, double py)
    {
        return px >= X && px <= X + Width && py >= Y && py <= Y + Height;
    }
}

public static class TranslationZoneManager
{
    public static List<TranslationZone> ActiveZones { get; set; } = new();
    public static bool HasActiveZones => ActiveZones.Count > 0;
}
