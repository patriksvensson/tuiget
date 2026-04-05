namespace Tuiget;

public record TickSource
{
    private static int _nextId;

    /// <summary>
    /// Gets the unique identifier for this tick source.
    /// </summary>
    public int Id { get; init; } = Interlocked.Increment(ref _nextId);

    /// <summary>
    /// Gets the generation tag. Incremented on state changes to invalidate pending ticks.
    /// </summary>
    public int Tag { get; init; }

    /// <summary>
    /// Returns a new <see cref="TickSource"/> with <see cref="Tag"/> incremented by one,
    /// preserving <see cref="Id"/>.
    /// </summary>
    public TickSource Advance()
    {
        return this with
        {
            Tag = Tag + 1,
        };
    }

    /// <summary>
    /// Returns <c>true</c> if the tick message matches this source's <see cref="Id"/>
    /// and <see cref="Tag"/>.
    /// </summary>
    public bool IsValid(TickMessage tick)
    {
        return tick.Id == Id && tick.Tag == Tag;
    }

    /// <summary>
    /// Returns <c>true</c> if the tick message matches this source's <see cref="Id"/>,
    /// <see cref="Tag"/>, and the specified <paramref name="kind"/>.
    /// </summary>
    public bool IsValid(TickMessage tick, string kind)
    {
        return tick.Id == Id && tick.Tag == Tag && tick.Kind == kind;
    }

    /// <summary>
    /// Creates a <see cref="TeaCommand"/> that waits for the specified interval then produces
    /// a <see cref="TickMessage"/> with this source's current <see cref="Id"/> and <see cref="Tag"/>.
    /// </summary>
    public TeaCommand CreateTick(TimeSpan interval, string? kind = null)
    {
        var id = Id;
        var tag = Tag;

        return TeaCommands.Tick(
            interval,
            time => new TickMessage
            {
                Time = time,
                Id = id,
                Tag = tag,
                Kind = kind
            });
    }
}