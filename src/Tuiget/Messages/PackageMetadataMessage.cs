using NuGet.Protocol.Core.Types;

namespace Tuiget;

public record PackageMetadataMessage(IPackageSearchMetadata Metadata) : TeaMessage;