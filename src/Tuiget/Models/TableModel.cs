using NuGet.Packaging.Core;
using Spectre.Console;
using Spectre.Tui;

namespace Tuiget;

public sealed class TableModel : TeaModel
{
    private bool _hasFocus;
    private string? _query;
    private TableWidget<TableItem> _table;
    private readonly Debouncer _debouncer;

    public TableModel()
    {
        _debouncer = new Debouncer();
        _table = new TableWidget<TableItem>()
            .AutoAddColumns()
            .WrapAround()
            .ShowHeader(false)
            .SelectedIndex(0);
    }

    public override TeaCommand? Update(TeaMessage message)
    {
        if (message is FocusMessage focus)
        {
            _hasFocus = focus.Focus == Focus.List;
            _table.SelectedIndex = focus.Focus == Focus.List ? 0 : null;
        }

        if (message is ExecuteQueryMessage query)
        {
            _query = query.Query;
            return Query;
        }

        if (message is ShowQueryResultMessage result)
        {
            _table.Rows(result.Items);
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
                    _table.MoveDown();
                    break;
                case ConsoleKey.UpArrow:
                    _table.MoveUp();
                    break;
                case ConsoleKey.Enter:
                    var identity = _table.SelectedItem?.Identity.Id;
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
        var box = _hasFocus
            ? new BoxWidget()
                .TitlePadding(1)
                .MarkupTitle("Results")
            : new BoxWidget(new Style(Color.Gray))
                .TitlePadding(1)
                .MarkupTitle("Results");

        if (_table.Rows.Count > 0)
        {
            ctx.Render(
                box.Inner(
                    new CompositeWidget(
                        new ClearWidget(' ', new Style(decoration: Decoration.Bold)),
                        new PaddingWidget(new Padding(1, 0, 2, 0), _table),
                        new ScrollbarWidget()
                            .VerticalRight()
                            .Position(_table.SelectedIndex ?? 0).Length(_table.Rows.Count)
                            .ViewportLength(1)
                            .Style(Color.Gray)
                            .ThumbStyle(Color.Green))));
        }
        else
        {
            ctx.Render(
                box.Inner(
                    new PaddingWidget(new Padding(1, 1, 0, 0),
                        Paragraph.FromMarkup("[gray]No search results available[/]"))));
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

        var items = result.Select(x => new TableItem(x.Identity));
        return new ShowQueryResultMessage(items.ToList());
    }

    private async Task<TeaMessage?> GetPackageMetadata(CancellationToken cancellationToken)
    {
        var selected = _table.SelectedItem;
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