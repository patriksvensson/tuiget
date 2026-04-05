using NuGet.Packaging.Core;
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