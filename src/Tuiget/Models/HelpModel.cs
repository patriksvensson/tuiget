using Spectre.Console;
using Spectre.Tui;

namespace Tuiget;

public sealed class HelpModel : TeaModel
{
    private readonly SpinnerModel _spinner = new();
    private Focus _currentFocus = Focus.Search;
    private bool _showSpinner;

    public override TeaCommand? Init()
    {
        return _spinner.Init();
    }

    public override TeaCommand? Update(TeaMessage message)
    {
        switch (message)
        {
            case FocusMessage focus:
                _currentFocus = focus.Focus;
                break;
            case ShowQueryResultMessage:
            case PackageMetadataMessage:
                _showSpinner = false;
                break;
            case ExecuteQueryMessage:
            case LoadingMetadataMessage:
                _showSpinner = true;
                break;
        }

        return message.Forward(_spinner);
    }

    public override void Render(RenderContext ctx)
    {
        ctx.Render(
            _currentFocus == Focus.Search
                ? Text.FromMarkup("[bold][[ESC]][/]:Quit  [bold][[Tab]][/]:Switch  [bold][[Enter]][/]:Search",
                    new Style(Color.Gray))
                : Text.FromMarkup(
                    "[bold][[ESC]][/]:Quit  [bold][[↑↓]][/]:Move  [bold][[Tab]][/]:Switch  [bold][[Enter]][/]:Select",
                    new Style(Color.Gray)));

        if (_showSpinner)
        {
            _spinner.X = ctx.Viewport.Width - 1;
            _spinner.Y = 0;
            ctx.Render(_spinner);
        }
    }
}