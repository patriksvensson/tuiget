using NuGet.Common;
using NuGet.Frameworks;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;

namespace Tuiget;

public static class NuGetService
{
    public static async Task<List<IPackageSearchMetadata>> Search(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return [];
        }

        var repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");
        var resource = await repository.GetResourceAsync<PackageSearchResource>();
        var searchFilter = new SearchFilter(includePrerelease: false)
        {
            IncludeDelisted = false,
            SupportedFrameworks = [FrameworkConstants.CommonFrameworks.Net50.Framework],
        };

        return (await resource.SearchAsync(
            query,
            searchFilter,
            skip: 0,
            take: 100,
            NullLogger.Instance,
            CancellationToken.None)).ToList();
    }

    public static async Task<IPackageSearchMetadata?> GetPackageInfo(PackageIdentity identity)
    {
        var repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");
        var resource = await repository.GetResourceAsync<PackageMetadataResource>();
        return await resource.GetMetadataAsync(identity, new NullSourceCacheContext(), new NullLogger(), CancellationToken.None);
    }
}