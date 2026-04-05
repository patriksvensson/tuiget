using Spectre.Tui;

namespace Tuiget;

public class HeaderModel : TeaModel
{
    public override TeaCommand? Update(TeaMessage message)
    {
        return null;
    }

    public override void Render(RenderContext ctx)
    {
        ctx.Render(Text.FromMarkup("[[[[ [blue]NuGet TUI[/] ]]]]"));
    }
}