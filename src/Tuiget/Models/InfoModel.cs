using NuGet.Protocol.Core.Types;
using Spectre.Console;
using Spectre.Tui;

namespace Tuiget;

public class InfoModel : TeaModel
{
    private IPackageSearchMetadata? _metadata;

    public override TeaCommand? Update(TeaMessage message)
    {
        if (message is PackageMetadataMessage metadata)
        {
            _metadata = metadata.Metadata;
        }
        else if (message is ShowQueryResultMessage or ExecuteQueryMessage)
        {
            _metadata = null;
        }

        return null;
    }

    public override void Render(RenderContext ctx)
    {
        ctx.Render(new BoxWidget(new Style(Color.Gray))
            .TitlePadding(1)
            .MarkupTitle("Info"));

        if (_metadata == null)
        {
            ctx.Render(
                new ClearWidget(' ', new Style(Color.Gray)), ctx.Viewport.Inflate(-1, -1));
        }
        else
        {
            var inner = ctx.Viewport.Inflate(-1, -1);
            ctx.Render(new ClearWidget(' '), inner);

            var title = _metadata.Title.RemoveMarkup();
            var authors = _metadata.Authors.RemoveMarkup();
            var license = (_metadata.LicenseMetadata?.License ?? "Unknown").RemoveMarkup();
            var description = _metadata.Description.RemoveMarkup();

            ctx.Render(
                new PaddingWidget(
                    new Padding(1, 0, 1, 0),
                    Paragraph.FromMarkup(
                        $"""
                         [yellow]{title}[/]

                         [gray]Authors:[/]
                         {authors}

                         [gray]License:[/]
                         {license}

                         [gray]Description:[/]
                         {description}
                         """)), inner);
        }
    }
}