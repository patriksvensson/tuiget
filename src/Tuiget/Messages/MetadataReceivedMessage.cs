using NuGet.Protocol.Core.Types;

namespace Tuiget.Events;

public sealed record MetadataReceivedMessage : ApplicationMessage
{
    public required IPackageSearchMetadata Metadata { get; init; }
}