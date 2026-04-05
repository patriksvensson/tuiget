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
        ctx.Render(new BoxWidget(new Style(Color.Gray)));

        if (_metadata == null)
        {
            ctx.Render(new ClearWidget(' ', new Style(Color.Gray)), ctx.Viewport.Inflate(-1, -1));
        }
        else
        {
            ctx.Render(new ClearWidget(' '), ctx.Viewport.Inflate(-1, -1));

            ctx.SetString(2, 1, _metadata.Title, new Style(Color.Yellow));
            ctx.SetString(2, 3, "Authors:", new Style(Color.Gray));
            ctx.SetString(2, 4, _metadata.Authors[..Math.Min(ctx.Viewport.Width - 5, _metadata.Authors.Length)]);
            ctx.SetString(2, 6, "License:", new Style(Color.Gray));
            ctx.SetString(2, 7, _metadata.LicenseMetadata?.License ?? "Unknown");
            ctx.SetString(2, 9, "Description:", new Style(Color.Gray));
            ctx.SetString(2, 10, _metadata.Description[..Math.Min(ctx.Viewport.Width - 5, _metadata.Description.Length)] ?? "None");
        }
    }
}