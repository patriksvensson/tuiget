namespace Tuiget;

public abstract record TeaMessage;

public record QuitMessage : TeaMessage;

public record KeyMessage(ConsoleKeyInfo Data) : TeaMessage;

public record ReadyMessage : TeaMessage;

public record ResizeMessage : TeaMessage;

public record ErrorMessage(Exception Exception) : TeaMessage;

internal record SequenceMessage : TeaMessage
{
    /// <summary>
    /// Gets the message from the current step.
    /// </summary>
    public required TeaMessage? StepMessage { get; init; }

    /// <summary>
    /// Gets the remaining commands to execute.
    /// </summary>
    public required IReadOnlyList<TeaCommand> Remaining { get; init; }
}

public record TickMessage : TeaMessage
{
    public required DateTimeOffset Time { get; init; }
    public required int Id { get; init; }
    public required int Tag { get; init; }
    public string? Kind { get; init; }
}