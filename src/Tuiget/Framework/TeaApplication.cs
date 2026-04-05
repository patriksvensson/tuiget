using System.Threading.Channels;
using Spectre.Tui;

namespace Tuiget;

public static class TeaApplication
{
    public static async Task RunAsync<TModel>(
        TModel model,
        CancellationToken cancellationToken) where TModel : TeaModel
    {
        var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        using var terminal = Terminal.Create();
        var renderer = new Renderer(terminal);

        Console.Title = "Tuiget";

        var channel = Channel.CreateUnbounded<TeaMessage>(
            new UnboundedChannelOptions
            {
                SingleReader = true,
            });

        var rx = channel.Reader;
        var tx = channel.Writer;

        var init = model.Init();
        if (init != null)
        {
            ScheduleCommand(init, channel.Writer, cts.Token);
        }

        // Trigger initial render
        await channel.Writer.WriteAsync(new ReadyMessage(), cts.Token);

        // Start input handler
        var inputReader = new InputReader();
        var inputPump = RunInputPumpAsync(inputReader, tx, cts.Token);

        try
        {
            await foreach (var message in rx.ReadAllAsync(cts.Token))
            {
                var quit = ProcessMessage(model, message, tx, cts.Token);
                if (quit)
                {
                    break;
                }

                renderer.Draw((ctx, _) =>
                {
                    model.Render(ctx);
                });
            }
        }
        catch (OperationCanceledException) when (cts.Token.IsCancellationRequested)
        {
            // Expected on shutdown
        }

        // Stop input pump and wait for it to complete
        await cts.CancelAsync();
        channel.Writer.Complete();
        await inputPump;
    }

    private static bool ProcessMessage<TModel>(
        TModel model, TeaMessage message,
        ChannelWriter<TeaMessage> tx,
        CancellationToken cancellationToken) where TModel : TeaModel
    {
        switch (message)
        {
            case SequenceMessage seq:
                var seqQuit = false;
                if (seq.StepMessage is not null)
                {
                    seqQuit = ProcessMessage(model, seq.StepMessage, tx, cancellationToken);
                }

                if (!seqQuit && seq.Remaining.Count > 0)
                {
                    var nextCmd = TeaCommands.Sequence(
                        seq.Remaining.Select(c => (TeaCommand?)c).ToArray());

                    if (nextCmd is not null)
                    {
                        ScheduleCommand(nextCmd, tx, cancellationToken);
                    }
                }

                return seqQuit;

            default:
                var isQuit = message is QuitMessage;
                var command = model.Update(message);
                if (command != null && !isQuit)
                {
                    ScheduleCommand(command, tx, cancellationToken);
                }

                return isQuit;
        }
    }

    private static void ScheduleCommand(
        TeaCommand command,
        ChannelWriter<TeaMessage> tx,
        CancellationToken ct)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                var result = await command(ct);
                if (result is not null)
                {
                    await tx.WriteAsync(result, ct);
                }
            }
            catch (OperationCanceledException)
            {
                // Expected on shutdown
            }
            catch (Exception ex)
            {
                try
                {
                    await tx.WriteAsync(
                        new ErrorMessage(ex),
                        CancellationToken.None);
                }
                catch (ChannelClosedException)
                {
                    // Channel already closed during shutdown
                }
            }
        }, ct);
    }

    private static async Task RunInputPumpAsync(
        InputReader reader,
        ChannelWriter<TeaMessage> rx,
        CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var message = await reader.ReadAsync(cancellationToken);
                if (message is not null)
                {
                    await rx.WriteAsync(message, cancellationToken);
                }
                else
                {
                    await Task.Delay(10, cancellationToken);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected on shutdown
        }
        catch (ChannelClosedException)
        {
            // Channel closed during shutdown
        }
    }
}