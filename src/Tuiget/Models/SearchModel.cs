using Spectre.Console;
using Spectre.Tui;

namespace Tuiget;

public sealed class SearchModel : TeaModel
{
    private bool _hasFocus = true;
    private string _query = string.Empty;

    public override TeaCommand? Update(TeaMessage message)
    {
        if (message is FocusMessage focus)
        {
            _hasFocus = focus.Focus == Focus.Search;
            return null;
        }


        if (message is KeyMessage key && _hasFocus)
        {
            if (key.Data is { Key: ConsoleKey.Backspace, Modifiers: ConsoleModifiers.Control })
            {
                _query = string.Empty;
            }
            else if (key.Data.Key == ConsoleKey.Backspace)
            {
                if (_query.Length > 0)
                {
                    _query = _query[..^1];
                }
            }
            else if (!char.IsControl(key.Data.KeyChar))
            {
                _query += key.Data.KeyChar;
            }

            return TeaCommands.Message(new ExecuteQueryMessage(_query));
        }

        return null;
    }

    public override void Render(RenderContext context)
    {
        context.Render(
            _hasFocus
                ? new BoxWidget()
                : new BoxWidget(new Style(Color.Gray)));

        context.SetString(2, 1, "Search:", new Style(Color.Gray));
        context.SetString(10, 1, _query, _hasFocus ? new Style(Color.Yellow) : new Style(Color.Gray));

        if (_hasFocus)
        {
            context.SetCursorPosition(new Position(10 + _query.Length, 2));
        }
    }
}