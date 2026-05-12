using Spectre.Console;
using Tuiget.Events;
using Tuiget.Framework;

namespace Tuiget;

public class PackageComponent : ScreenComponent, IKeyMap
{
    private readonly TableWidget<PackageTableItem> _table;
    private readonly KeyBinding _select = KeyBinding.For(Key.Enter).WithHelp("Select");

    public PackageComponent(MainScreen parent)
        : base(parent)
    {
        _table = new TableWidget<PackageTableItem>()
            .AutoAddColumns()
            .WrapAround()
            .ShowHeader(false)
            .HighlightStyle(new Style(decoration: Decoration.Invert));
    }

    public override void OnEvent(ApplicationContext context, ApplicationMessage message)
    {
        if (message is SearchCompletedMessage result)
        {
            _table.Rows(result.Packages);

            if (result.Packages.Count > 0)
            {
                _table.SelectedIndex(0);
                Focus();
            }
        }

        if (message is MetadataReceivedMessage metadata)
        {
            context.Push(new PopupScreen(
                new Size(80, 24),
                metadata.Metadata.Title,
                new PackagePopup(metadata.Metadata)));
        }

        if (IsFocused && message is KeyMessage key)
        {
            _table.KeyMap.HandleKey(key);

            if (_select.Matches(key) && _table.SelectedItem != null)
            {
                context.Broadcast(new ShowProgressMessage());
                context.StartJob(async job =>
                {
                    var metadata = await NuGetService.GetPackageInfo(_table.SelectedItem.Identity);
                    if (metadata != null)
                    {
                        job.Broadcast(new MetadataReceivedMessage
                        {
                            Metadata = metadata,
                        });
                    }
                });
            }
        }
    }

    public override void Render(RenderContext context)
    {
        var box = IsFocused
            ? new BoxWidget()
                .TitlePadding(1)
                .MarkupTitle("Results")
            : new BoxWidget(new Style(Color.Gray))
                .TitlePadding(1)
                .MarkupTitle("Results");

        if (_table.Rows.Count > 0)
        {
            context.Render(
                box.Inner(
                    new CompositeWidget(
                        new ClearWidget(' ', new Style(decoration: Decoration.Bold)),
                        new PaddingWidget(new Padding(1, 0, 2, 0), _table),
                        new ScrollbarWidget()
                            .VerticalRight()
                            .Position(_table.SelectedIndex ?? 0).Length(_table.Rows.Count)
                            .ViewportLength(1))));
        }
        else
        {
            context.Render(
                box.Inner(
                    new PaddingWidget(new Padding(1, 1, 0, 0),
                        Paragraph.FromMarkup("[gray]No search results available[/]"))));
        }
    }

    public IEnumerable<KeyBinding> Help()
    {
        yield return _select;
    }
}