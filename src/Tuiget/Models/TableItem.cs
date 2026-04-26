using NuGet.Packaging.Core;
using Spectre.Tui;

namespace Tuiget;

public record TableItem(PackageIdentity Identity)
    : ITableRow, ITableColumnDefinition
{
    public Text[] CreateCells(bool isSelected)
    {
        if (isSelected)
        {
            return
            [
                Text.FromMarkup($"[u blue]{Identity.Id}[/]"),
                Text.FromMarkup($"{Identity.Version}"),
            ];
        }
        else
        {
            return
            [
                Text.FromMarkup($"[yellow]{Identity.Id}[/]"),
                Text.FromMarkup($"[gray]{Identity.Version}[/]"),
            ];
        }
    }

    public static IEnumerable<TableColumn> GetColumns()
    {
        return
        [
            new TableColumn("Package").StarWidth(1),
            new TableColumn("Version").FixedWidth(7).RightAligned(),
        ];
    }
}