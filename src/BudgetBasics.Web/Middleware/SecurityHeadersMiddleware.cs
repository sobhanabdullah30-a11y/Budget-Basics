using BudgetBasics.Web.Configuration;
using Microsoft.Extensions.Options;

namespace BudgetBasics.Web.Middleware;

public sealed class SecurityHeadersMiddleware(
    RequestDelegate next,
    IWebHostEnvironment environment,
    IOptions<CdnOptions> cdnOptions)
{
    private readonly string _contentSecurityPolicy = BuildContentSecurityPolicy(
        environment.IsDevelopment(),
        cdnOptions.Value);

    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            var headers = context.Response.Headers;
            headers["X-Content-Type-Options"] = "nosniff";
            headers["X-Frame-Options"] = "DENY";
            headers["X-XSS-Protection"] = "0";
            headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=(), payment=(), usb=()";
            headers["Cross-Origin-Opener-Policy"] = "same-origin";
            headers["Cross-Origin-Resource-Policy"] = "same-origin";
            headers["Content-Security-Policy"] = _contentSecurityPolicy;
            headers.Remove("Server");
            headers.Remove("X-Powered-By");

            if (context.User.Identity?.IsAuthenticated == true || IsSensitivePath(context.Request.Path))
            {
                headers.CacheControl = "no-store, no-cache, max-age=0";
                headers.Pragma = "no-cache";
                headers.Expires = "0";
            }

            return Task.CompletedTask;
        });

        await next(context);
    }

    private static bool IsSensitivePath(PathString path) =>
        path.StartsWithSegments("/Account", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWithSegments("/tools/expenses", StringComparison.OrdinalIgnoreCase);

    private static string BuildContentSecurityPolicy(bool isDevelopment, CdnOptions cdn)
    {
        var assetOrigin = cdn.Enabled
            ? $" {new Uri(cdn.BaseUrl).GetLeftPart(UriPartial.Authority)}"
            : string.Empty;
        var policy =
            "default-src 'self'; " +
            "base-uri 'self'; " +
            "connect-src 'self'; " +
            $"font-src 'self' data:{assetOrigin}; " +
            "form-action 'self'; " +
            "frame-ancestors 'none'; " +
            $"img-src 'self' data:{assetOrigin}; " +
            "object-src 'none'; " +
            $"script-src 'self'{assetOrigin}; " +
            $"style-src 'self' 'unsafe-inline'{assetOrigin};";

        return isDevelopment ? policy : $"{policy} upgrade-insecure-requests;";
    }
}
