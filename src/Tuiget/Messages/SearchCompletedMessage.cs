namespace Tuiget.Events;

public sealed record SearchCompletedMessage : ApplicationMessage
{
    public required List<PackageTableItem> Packages { get; init; }
}