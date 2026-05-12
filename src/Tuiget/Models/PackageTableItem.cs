using NuGet.Packaging.Core;

namespace Tuiget;

public record PackageTableItem(PackageIdentity Identity) : ITableRow, ITableColumnDefinition
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
            new TableColumn("Version").FixedWidth(15).RightAligned(),
        ];
    }
}