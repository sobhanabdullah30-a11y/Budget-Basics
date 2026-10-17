using BudgetBasics.Web.Configuration;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Options;

namespace BudgetBasics.Web.Services;

public interface IAssetUrlProvider
{
    string GetUrl(string path);
}

public sealed class AssetUrlProvider(
    IFileVersionProvider fileVersionProvider,
    IOptions<CdnOptions> cdnOptions) : IAssetUrlProvider
{
    private readonly CdnOptions _cdn = cdnOptions.Value;

    public string GetUrl(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var applicationPath = $"/{path.TrimStart('~', '/')}";
        var versionedPath = fileVersionProvider.AddFileVersionToPath(PathString.Empty, applicationPath);

        return _cdn.Enabled
            ? $"{_cdn.BaseUrl.TrimEnd('/')}{versionedPath}"
            : versionedPath;
    }
}
