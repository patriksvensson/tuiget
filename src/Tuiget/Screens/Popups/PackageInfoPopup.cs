using NuGet.Protocol.Core.Types;
using Spectre.Console;
using Size = Spectre.Tui.Size;

namespace Tuiget;

public sealed class PackagePopup : Screen
{
    private readonly IPackageSearchMetadata _metadata;
    private readonly ScrollViewWidget _scroll;

    public PackagePopup(IPackageSearchMetadata metadata)
    {
        _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        _scroll = new ScrollViewWidget().HorizontalScroll(ScrollMode.Disabled);
    }

    public override void OnMessage(ApplicationContext context, ApplicationMessage message)
    {
        if (message is KeyMessage key)
        {
            if (key.Character == 'q')
            {
                context.Pop();
                return;
            }

            switch (key.Key)
            {
                case Key.Down:
                    _scroll.ScrollDown();
                    break;
                case Key.Up:
                    _scroll.ScrollUp();
                    break;
            }
        }
    }

    public override void Render(RenderContext context)
    {
        var authors = _metadata.Authors.RemoveMarkup();
        var license = (_metadata.LicenseMetadata?.License ?? "Unknown").RemoveMarkup();
        var description = _metadata.Description.RemoveMarkup();

        context.Render(
            _scroll
                .Inner(
                    new PaddingWidget(new Padding(1, 0),
                        Paragraph.FromMarkup(
                            $"""
                             [gray]Description:[/]
                             {description}

                             [gray]Authors:[/]
                             {authors}

                             [gray]License:[/]
                             {license}
                             """
                        ))));
    }
}