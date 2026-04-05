using Spectre.Tui;

namespace Tuiget;

public sealed class SpinnerModel : TeaModel
{
    public int X { get; set; }
    public int Y { get; set; }

    public int Frame { get; set; }
    public SpinnerType Spinner { get; set; } = SpinnerType.Pulse;

    public TickSource Ticks { get; init; } = new();

    public override TeaCommand? Init()
    {
        return Ticks.CreateTick(Spinner.Interval);
    }

    public override TeaCommand? Update(TeaMessage message)
    {
        switch (message)
        {
            case TickMessage tick when Ticks.IsValid(tick):
                Frame = (Frame + 1) % Spinner.Frames.Count;
                return Ticks.CreateTick(Spinner.Interval);
        }

        return null;
    }

    public override void Render(RenderContext ctx)
    {
        var frame = Spinner.Frames.Count > 0
            ? Spinner.Frames[Frame % Spinner.Frames.Count]
            : string.Empty;

        ctx.SetString(X, Y, frame);
    }
}

public record SpinnerType(IReadOnlyList<string> Frames, TimeSpan Interval)
{
    public static SpinnerType Pulse { get; } = new(
        ["\u2588", "\u2593", "\u2592", "\u2591", " ", "\u2591", "\u2592", "\u2593"],
        TimeSpan.FromMilliseconds(120));

    public static SpinnerType Meter { get; } = new(
        ["\u2581", "\u2582", "\u2583", "\u2584", "\u2585", "\u2586", "\u2587", "\u2588", "\u2587", "\u2586", "\u2585", "\u2584", "\u2583", "\u2582"],
        TimeSpan.FromMilliseconds(100));

    public static SpinnerType Points { get; } = new(
        ["\u2219", "\u2219\u2219", "\u2219\u2219\u2219", " "],
        TimeSpan.FromMilliseconds(300));
}