using Spectre.Tui;

namespace Tuiget;

public abstract class TeaModel : IWidget
{
    public virtual TeaCommand? Init()
    {
        return null;
    }

    public abstract TeaCommand? Update(TeaMessage message);
    public abstract void Render(RenderContext ctx);
}

public static class MessageExtensions
{
    public static TeaCommand? Forward(this TeaMessage message, params TeaModel[] models)
    {
        var result = default(TeaCommand?);
        foreach (var model in models)
        {
            var command = model.Update(message);
            if (command == null)
            {
                continue;
            }

            result = result != null
                ? TeaCommands.Sequence(result, command)
                : command;
        }

        return result;
    }
}