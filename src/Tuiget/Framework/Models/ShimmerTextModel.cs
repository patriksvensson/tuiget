using Spectre.Console;
using Spectre.Tui;

namespace Tuiget;

public sealed class ShimmerTextModel : TeaModel
{
    private readonly ShimmerTextWidget _widget = new();

    public string Text
    {
        get => _widget.Text;
        set => _widget.Text = value;
    }

    public double BaseHue
    {
        get => _widget.BaseHue;
        set => _widget.BaseHue = value;
    }

    public double WaveLength
    {
        get => _widget.WaveLength;
        set => _widget.WaveLength = value;
    }

    public double WaveAmplitude
    {
        get => _widget.WaveAmplitude;
        set => _widget.WaveAmplitude = value;
    }

    public double HueAmplitude
    {
        get => _widget.HueAmplitude;
        set => _widget.HueAmplitude = value;
    }

    public double Saturation
    {
        get => _widget.Saturation;
        set => _widget.Saturation = value;
    }

    public double Lightness
    {
        get => _widget.Lightness;
        set => _widget.Lightness = value;
    }

    public Justify Alignment
    {
        get => _widget.Alignment;
        set => _widget.Alignment = value;
    }

    public Decoration Decoration
    {
        get => _widget.Decoration;
        set => _widget.Decoration = value;
    }

    public TimeSpan Interval { get; set; } = TimeSpan.FromMilliseconds(80);
    public double Speed { get; set; } = 0.03;

    public TickSource Ticks { get; init; } = new();

    public override TeaCommand? Init()
    {
        return Ticks.CreateTick(Interval);
    }

    public override TeaCommand? Update(TeaMessage message)
    {
        switch (message)
        {
            case TickMessage tick when Ticks.IsValid(tick):
                var next = (_widget.Phase + Speed) % 1.0;
                if (next < 0)
                {
                    next += 1.0;
                }

                _widget.Phase = next;
                return Ticks.CreateTick(Interval);
        }

        return null;
    }

    public override void Render(RenderContext ctx)
    {
        ctx.Render(_widget);
    }
}
