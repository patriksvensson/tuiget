using NuGet.Packaging.Core;
using Spectre.Console;
using Spectre.Tui;

namespace Tuiget;

public record ListItem(PackageIdentity Identity) : IListWidgetItem
{
    public Text CreateText(bool isSelected)
    {
        if (isSelected)
        {
            return Text.FromMarkup($" [u blue]{Identity.Id}[/] {Identity.Version}");
        }

        return Text.FromMarkup($" [yellow]{Identity.Id}[/] [gray]{Identity.Version}[/]");
    }
}

public sealed class ListModel : TeaModel
{
    private bool _hasFocus;
    private string? _query;
    private Spectre.Tui.ListWidget<ListItem> _list;
    private readonly Debouncer _debouncer;

    public ListModel()
    {
        _debouncer = new Debouncer();
        _list = new ListWidget<ListItem>()
            .HighlightSymbol("→ ")
            .WrapAround();
    }

    public override TeaCommand? Update(TeaMessage message)
    {
        if (message is FocusMessage focus)
        {
            _hasFocus = focus.Focus == Focus.List;
            _list.SelectedIndex = focus.Focus == Focus.List ? 0 : null;
        }

        if (message is ExecuteQueryMessage query)
        {
            _query = query.Query;
            return Query;
        }

        if (message is ShowQueryResultMessage result)
        {
            _list.WithItems(result.Items);
        }

        if (message is LoadingMetadataMessage loading)
        {
            return GetPackageMetadata;
        }

        if (message is KeyMessage key && _hasFocus)
        {
            switch (key.Data.Key)
            {
                case ConsoleKey.DownArrow:
                    _list.MoveDown();
                    break;
                case ConsoleKey.UpArrow:
                    _list.MoveUp();
                    break;
                case ConsoleKey.Enter:
                    var identity = _list.SelectedItem?.Identity.Id;
                    if (!string.IsNullOrWhiteSpace(identity))
                    {
                        return TeaCommands.Message(
                            new LoadingMetadataMessage(identity));
                    }
                    break;
            }
        }

        return null;
    }

    public override void Render(RenderContext ctx)
    {
        ctx.Render(
            _hasFocus
                ? new BoxWidget()
                : new BoxWidget(new Style(Color.Gray)));

        if (_list.Items.Count > 0)
        {
            ctx.Render(_list, ctx.Viewport.Inflate(-1, -1));
        }
        else
        {
            ctx.SetString(2, 1, "No search results available", new Style(Color.Gray));
        }
    }

    private async Task<TeaMessage?> Query(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_query))
        {
            return new ShowQueryResultMessage([]);
        }

        var result = await _debouncer.DebounceAsync(200, async (ct) =>
        {
            return await NuGetService.Search(_query);
        });

        if (result == null)
        {
            return null;
        }

        var items = result.Select(x => new ListItem(x.Identity));
        return new ShowQueryResultMessage(items.ToList());
    }

    private async Task<TeaMessage?> GetPackageMetadata(CancellationToken cancellationToken)
    {
        var selected = _list.SelectedItem;
        if (selected == null)
        {
            return null;
        }

        var result = await NuGetService.GetPackageInfo(selected.Identity);
        if (result == null)
        {
            return null;
        }

        return new PackageMetadataMessage(result);
    }
}