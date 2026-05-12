using Spectre.Console;
using Tuiget.Events;
using Tuiget.Framework;

namespace Tuiget;

public class MainScreen : Screen, IKeyMap
{
    private readonly Layout _layout;
    private readonly SearchComponent _search;
    private readonly PackageComponent _packages;
    private readonly ProgressBarWidget _progress;
    private readonly FocusRing _focus;
    private readonly KeyBinding _quit = KeyBinding.For('q').WithHelp("Quit");

    private bool _searching = false;

    public MainScreen()
    {
        _layout = new Layout("root")
            .SplitRows(
                new Layout("search").Size(3),
                new Layout("packages"),
                new Layout("progress").Size(1).Hidden(),
                new Layout("help").Size(1));

        _search = new SearchComponent(this);
        _packages = new PackageComponent(this);
        _progress = new ProgressBarWidget()
            .Value(100).HideLabel()
            .Foreground(ProgressBarBrush.Wave(
                new Color(75, 75, 75), new Color(200, 200, 200),
                TimeSpan.FromSeconds(1)));

        _focus = new FocusRing([_search, _packages]);
    }

    public override void OnMessage(ApplicationContext context, ApplicationMessage message)
    {
        _focus.HandleInput(message);

        switch (message)
        {
            case ShowProgressMessage show:
                _layout.GetLayout("progress").Visible();
                _searching = true;
                break;
            case SearchCompletedMessage or MetadataReceivedMessage:
                _layout.GetLayout("progress").Hidden();
                _searching = false;
                break;
            case KeyMessage key:
                if (_quit.Matches(key))
                {
                    context.Pop();
                    return;
                }
                break;
        }

        _search.OnEvent(context, message);
        _packages.OnEvent(context, message);
    }

    public override void Update(FrameInfo frame, IRenderBounds bounds)
    {
        _progress.Update(frame);
    }

    public override void Render(RenderContext context)
    {
        context.Render(_search, _layout.GetArea(context, "search"));
        context.Render(_packages, _layout.GetArea(context, "packages"));

        if (_searching)
        {
            context.Render(_progress, _layout.GetArea(context, "progress"));
        }

        context.Render(
            new HelpWidget(this),
            _layout.GetArea(context, "help"));
    }

    public void FocusComponent<T>(T item)
        where T : IFocusable
    {
        _focus.Focus(item);
    }

    public IEnumerable<KeyBinding> Help()
    {
        yield return _quit;

        if (_search.IsFocused)
        {
            foreach (var binding in _search.Help())
            {
                yield return binding;
            }
        }

        if (_packages.IsFocused)
        {
            foreach (var binding in _packages.Help())
            {
                yield return binding;
            }
        }
    }
}