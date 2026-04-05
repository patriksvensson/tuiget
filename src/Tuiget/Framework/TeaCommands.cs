namespace Tuiget;

public delegate Task<TeaMessage?> TeaCommand(CancellationToken cancellationToken);

public static class TeaCommands
{
    public static TeaCommand Message(TeaMessage message)
    {
        return _ => Task.FromResult<TeaMessage?>(message);
    }

    public static TeaCommand Quit()
    {
        return Message(new QuitMessage());
    }

    public static TeaCommand? Sequence(params TeaCommand?[] commands)
    {
        var filtered = commands.Where(c => c is not null).Cast<TeaCommand>().ToArray();
        if (filtered.Length == 0)
        {
            return null;
        }

        if (filtered.Length == 1)
        {
            return filtered[0];
        }

        return async ct =>
        {
            TeaMessage? first;
            try
            {
                first = await filtered[0](ct);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                first = new ErrorMessage(ex);
            }

            var remaining = filtered.Skip(1).ToList();
            return new SequenceMessage { StepMessage = first, Remaining = remaining };
        };
    }

    public static TeaCommand Tick(TimeSpan interval, Func<DateTimeOffset, TeaMessage> createMessage)
    {
        return async ct =>
        {
            await Task.Delay(interval, ct);
            return createMessage(DateTimeOffset.UtcNow);
        };
    }
}