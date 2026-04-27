using Spectre.Console;
using Spectre.Tui;

namespace Tuiget;

public sealed class ShimmerTextWidget : IWidget
{
    public string Text { get; set; } = string.Empty;
    public double Phase { get; set; }

    // Wave mode
    public double BaseHue { get; set; } = 200.0;
    public double WaveLength { get; set; } = 12.0;
    public double WaveAmplitude { get; set; } = 0.3;
    public double HueAmplitude { get; set; } = 10.0;
    public double Saturation { get; set; } = 1.0;
    public double Lightness { get; set; } = 0.6;
    public Justify Alignment { get; set; } = Justify.Left;
    public Decoration Decoration { get; set; } = Decoration.None;

    public void Render(RenderContext ctx)
    {
        if (string.IsNullOrEmpty(Text))
        {
            return;
        }

        var width = Text.Length;
        var startX = Alignment switch
        {
            Justify.Center => Math.Max(0, (ctx.Viewport.Width - width) / 2),
            Justify.Right => Math.Max(0, ctx.Viewport.Width - width),
            _ => 0,
        };

        var pos = new Position(startX, 0);
        var index = 0;
        foreach (var rune in Text.EnumerateRunes())
        {
            var (h, s, l) = GetWaveColor(index);
            var (r, g, b) = HslToRgb(h, s, l);
            var style = new Style(
                foreground: new Color(r, g, b),
                decoration: Decoration);

            pos = ctx.SetString(pos, rune.ToString(), style);
            index++;
        }
    }

    private (double H, double S, double L) GetWaveColor(int index)
    {
        var theta = (Phase * Math.PI * 2.0) - (index * Math.PI * 2.0 / WaveLength);
        var wave = Math.Sin(theta);
        var lightness = Math.Clamp(Lightness + (wave * WaveAmplitude), 0.05, 0.95);

        var hue = (BaseHue + (wave * HueAmplitude)) % 360.0;
        if (hue < 0)
        {
            hue += 360.0;
        }

        return (hue, Saturation, lightness);
    }

    private static (byte R, byte G, byte B) HslToRgb(double hue, double saturation, double lightness)
    {
        var c = (1 - Math.Abs((2 * lightness) - 1)) * saturation;
        var hPrime = hue / 60.0;
        var x = c * (1 - Math.Abs((hPrime % 2) - 1));
        var m = lightness - (c / 2);

        double r1, g1, b1;
        switch (hPrime)
        {
            case < 1:
                r1 = c;
                g1 = x;
                b1 = 0;
                break;
            case < 2:
                r1 = x;
                g1 = c;
                b1 = 0;
                break;
            case < 3:
                r1 = 0;
                g1 = c;
                b1 = x;
                break;
            case < 4:
                r1 = 0;
                g1 = x;
                b1 = c;
                break;
            case < 5:
                r1 = x;
                g1 = 0;
                b1 = c;
                break;
            default:
                r1 = c;
                g1 = 0;
                b1 = x;
                break;
        }

        var r = (byte)Math.Clamp((int)Math.Round((r1 + m) * 255), 0, 255);
        var g = (byte)Math.Clamp((int)Math.Round((g1 + m) * 255), 0, 255);
        var b = (byte)Math.Clamp((int)Math.Round((b1 + m) * 255), 0, 255);

        return (r, g, b);
    }
}