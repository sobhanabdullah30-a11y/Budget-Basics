using System.ComponentModel.DataAnnotations;

namespace BudgetBasics.Web.Configuration;

public sealed class RateLimitOptions
{
    public const string SectionName = "RateLimiting";

    [Range(10, 10_000)]
    public int GlobalPermitLimit { get; init; } = 300;

    [Range(1, 3_600)]
    public int GlobalWindowSeconds { get; init; } = 60;

    [Range(1, 20)]
    public int GlobalSegmentsPerWindow { get; init; } = 6;

    [Range(1, 100)]
    public int AuthenticationPermitLimit { get; init; } = 10;

    [Range(1, 3_600)]
    public int AuthenticationWindowSeconds { get; init; } = 60;

    [Range(1, 100)]
    public int AssistantPermitLimit { get; init; } = 10;

    [Range(1, 3_600)]
    public int AssistantWindowSeconds { get; init; } = 60;

    [Range(1, 1_000)]
    public int ApiPermitLimit { get; init; } = 60;

    [Range(1, 3_600)]
    public int ApiWindowSeconds { get; init; } = 60;
}

public sealed class CacheOptions
{
    public const string SectionName = "Caching";

    [Range(10, 86_400)]
    public int PublicPageSeconds { get; init; } = 300;

    [Range(10, 86_400)]
    public int ReferenceDataSeconds { get; init; } = 60;

    [Range(60, 31_536_000)]
    public int StaticAssetSeconds { get; init; } = 31_536_000;

    [Range(1, 500)]
    public int OutputCacheSizeMegabytes { get; init; } = 50;

    [Range(64, 10_240)]
    public int MaximumCachedResponseKilobytes { get; init; } = 1_024;
}

public sealed class CdnOptions : IValidatableObject
{
    public const string SectionName = "Cdn";

    public bool Enabled { get; init; }

    public string BaseUrl { get; init; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!Enabled)
        {
            yield break;
        }

        if (!Uri.TryCreate(BaseUrl, UriKind.Absolute, out var uri) ||
            !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) ||
            !string.IsNullOrEmpty(uri.Query) ||
            !string.IsNullOrEmpty(uri.Fragment))
        {
            yield return new ValidationResult(
                "Cdn:BaseUrl must be an absolute HTTPS origin without a query string or fragment when the CDN is enabled.",
                [nameof(BaseUrl)]);
        }
    }
}
