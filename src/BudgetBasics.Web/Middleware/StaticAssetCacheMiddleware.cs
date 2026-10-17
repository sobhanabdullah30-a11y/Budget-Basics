using BudgetBasics.Web.Configuration;
using Microsoft.Extensions.Options;

namespace BudgetBasics.Web.Middleware;

public sealed class StaticAssetCacheMiddleware(
    RequestDelegate next,
    IOptions<CacheOptions> cacheOptions)
{
    private readonly string _cacheControl =
        $"public,max-age={cacheOptions.Value.StaticAssetSeconds},immutable";

    public async Task InvokeAsync(HttpContext context)
    {
        if (IsStaticAsset(context.Request.Path))
        {
            context.Response.OnStarting(() =>
            {
                if (context.Response.StatusCode == StatusCodes.Status200OK)
                {
                    context.Response.Headers.CacheControl = _cacheControl;
                }

                return Task.CompletedTask;
            });
        }

        await next(context);
    }

    private static bool IsStaticAsset(PathString path) =>
        path.StartsWithSegments("/css", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWithSegments("/js", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWithSegments("/lib", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWithSegments("/images", StringComparison.OrdinalIgnoreCase) ||
        path.Equals("/favicon.ico", StringComparison.OrdinalIgnoreCase);
}
