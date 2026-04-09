using Spectre.Tui;

namespace Tuiget;

public sealed class MainModel : TeaModel
{
    private readonly Layout _layout;
    private readonly SearchModel _searchModel;
    private readonly ListModel _listModel;
    private readonly InfoModel _infoModel;
    private readonly HelpModel _helpModel;
    private Focus _currentFocus = Focus.Search;

    public MainModel()
    {
        _searchModel = new SearchModel();
        _listModel = new ListModel();
        _infoModel = new InfoModel();
        _helpModel = new HelpModel();

        _layout = new Layout("root")
            .SplitRows(
                new Layout("search").Size(3),
                new Layout("middle")
                    .SplitColumns(
                        new Layout("list"),
                        new Layout("info")),
                new Layout("help").Size(1));
    }

    public override TeaCommand? Init()
    {
        return _helpModel.Init();
    }

    public override TeaCommand? Update(TeaMessage message)
    {
        switch (message)
        {
            // Want to exit?
            case KeyMessage { Data.Key: ConsoleKey.Escape }:
                {
                    return TeaCommands.Quit();
                }
            case KeyMessage { Data.Key: ConsoleKey.Tab }:
                {
                    _currentFocus = _currentFocus == Focus.Search ? Focus.List : Focus.Search;
                    return TeaCommands.Message(new FocusMessage(_currentFocus));
                }
            default:
                if (message is FocusMessage focus)
                {
                    _currentFocus = focus.Focus;
                }

                return message.Forward(_searchModel, _listModel, _infoModel, _helpModel);
        }
    }

    public override void Render(RenderContext ctx)
    {
        ctx.Render(_searchModel, _layout.GetArea(ctx, "search"));
        ctx.Render(_listModel, _layout.GetArea(ctx, "list"));
        ctx.Render(_infoModel, _layout.GetArea(ctx, "info"));
        ctx.Render(_helpModel, _layout.GetArea(ctx, "help"));
    }
}