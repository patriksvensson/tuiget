using Spectre.Console;
using Tuiget.Events;
using Tuiget.Framework;

namespace Tuiget;

public class SearchComponent : ScreenComponent, IKeyMap
{
    private string _query = string.Empty;
    private KeyBinding _backspace = KeyBinding.For(Key.Backspace);
    private KeyBinding _clear = KeyBinding.For(KeyPress.For(Key.Backspace).WithCtrl());
    private KeyBinding _enter = KeyBinding.For(Key.Enter).WithHelp("Search");

    public SearchComponent(MainScreen parent)
        : base(parent)
    {
    }

    public override void OnEvent(ApplicationContext context, ApplicationMessage message)
    {
        if (!IsFocused)
        {
            return;
        }

        if (message is KeyMessage key)
        {
            if (_clear.Matches(key))
            {
                _query = string.Empty;
            }
            else if (_backspace.Matches(key))
            {
                if (_query.Length > 0)
                {
                    _query = _query[..^1];
                }
            }
            else if (_enter.Matches(key))
            {
                context.Broadcast(new ShowProgressMessage());

                // Execute
                context.StartJob(async job =>
                {
                    var result = await NuGetService.Search(_query);
                    var items = result
                        .ConvertAll(x => new PackageTableItem(x.Identity));

                    job.Broadcast(new SearchCompletedMessage
                    {
                        Packages = items,
                    });
                });
            }
            else if (key.Character != null && !char.IsControl(key.Character.Value))
            {
                _query += key.Character.Value;
            }
        }
    }

    public override void Render(RenderContext context)
    {
        context.Render(
            IsFocused
                ? new BoxWidget()
                    .TitlePadding(1)
                    .MarkupTitle("Search")
                : new BoxWidget(new Style(Color.Gray))
                    .TitlePadding(1)
                    .MarkupTitle("Search"));

        if (_query.Length > 0)
        {
            context.SetString(2, 1, _query, IsFocused ? new Style(Color.Yellow) : new Style(Color.Gray));
        }
        else
        {
            context.SetString(2, 1, "Package name", new Style(Color.Gray));
        }

        if (IsFocused)
        {
            context.SetCursorPosition(new Position(2 + _query.Length, 1));
        }
    }

    public IEnumerable<KeyBinding> Help()
    {
        yield return _enter;
    }
}