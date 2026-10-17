using BudgetBasics.Infrastructure;
using BudgetBasics.Infrastructure.Persistence;
using BudgetBasics.Web.Configuration;
using BudgetBasics.Web.Middleware;
using BudgetBasics.Web.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Options;
using System.IO.Compression;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// The Windows Event Log provider can throw AccessDenied on shared IIS hosts and
// can mask the original request error. Console output is captured by IIS/hosting
// logs and Debug remains available in Visual Studio.
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.ForwardLimit = 1;
    options.RequireHeaderSymmetry = true;
});
builder.Services.Configure<HstsOptions>(options =>
{
    options.MaxAge = TimeSpan.FromDays(365);
    options.IncludeSubDomains = true;
});

// Shared hosting can recycle the IIS worker process at any time. Persisting the
// Data Protection key ring keeps antiforgery and authentication cookies valid
// across those restarts instead of making every open form fail with HTTP 400.
var dataProtectionDirectory = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "DataProtection-Keys");
Directory.CreateDirectory(dataProtectionDirectory);
builder.Services.AddDataProtection()
    .SetApplicationName("BudgetBasics.Web")
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionDirectory));

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute()));
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "RequestVerificationToken";
    options.Cookie.Name = "__Host-BudgetBasics.Antiforgery";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
});
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});
builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
    options.Level = CompressionLevel.Fastest);
builder.Services.Configure<GzipCompressionProviderOptions>(options =>
    options.Level = CompressionLevel.Fastest);
builder.Services.AddHealthChecks();
builder.Services.AddHttpContextAccessor();

builder.Services.AddOptions<RateLimitOptions>()
    .Bind(builder.Configuration.GetSection(RateLimitOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddOptions<CacheOptions>()
    .Bind(builder.Configuration.GetSection(CacheOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddOptions<CdnOptions>()
    .Bind(builder.Configuration.GetSection(CdnOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddSingleton<IAssetUrlProvider, AssetUrlProvider>();

var rateLimits = builder.Configuration
    .GetSection(RateLimitOptions.SectionName)
    .Get<RateLimitOptions>() ?? new RateLimitOptions();
var cache = builder.Configuration
    .GetSection(CacheOptions.SectionName)
    .Get<CacheOptions>() ?? new CacheOptions();

builder.Services.AddOutputCache(options =>
{
    options.SizeLimit = cache.OutputCacheSizeMegabytes * 1024L * 1024L;
    options.MaximumBodySize = cache.MaximumCachedResponseKilobytes * 1024L;
    options.DefaultExpirationTimeSpan = TimeSpan.FromSeconds(cache.PublicPageSeconds);
    options.AddPolicy("public-pages", policy => policy
        .Expire(TimeSpan.FromSeconds(cache.PublicPageSeconds))
        .SetVaryByQuery("*")
        .Tag("public-pages"));
    options.AddPolicy("reference-data", policy => policy
        .Expire(TimeSpan.FromSeconds(cache.ReferenceDataSeconds))
        .SetVaryByQuery("habit")
        .Tag("reference-data"));
});
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, cancellationToken) =>
    {
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter =
                Math.Max(1, (int)Math.Ceiling(retryAfter.TotalSeconds)).ToString();
        }

        await context.HttpContext.Response.WriteAsJsonAsync(
            new { message = "Too many requests. Please wait briefly and try again." },
            cancellationToken);
    };
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetSlidingWindowLimiter(
            GetClientKey(context),
            _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = rateLimits.GlobalPermitLimit,
                Window = TimeSpan.FromSeconds(rateLimits.GlobalWindowSeconds),
                SegmentsPerWindow = rateLimits.GlobalSegmentsPerWindow,
                QueueLimit = 0,
                AutoReplenishment = true
            }));
    options.AddPolicy("authentication", context => RateLimitPartition.GetFixedWindowLimiter(
        partitionKey: GetClientKey(context),
        factory: _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = rateLimits.AuthenticationPermitLimit,
            Window = TimeSpan.FromSeconds(rateLimits.AuthenticationWindowSeconds),
            QueueLimit = 0,
            AutoReplenishment = true
        }));
    options.AddPolicy("assistant", context => RateLimitPartition.GetFixedWindowLimiter(
        partitionKey: GetClientKey(context),
        factory: _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = rateLimits.AssistantPermitLimit,
            Window = TimeSpan.FromSeconds(rateLimits.AssistantWindowSeconds),
            QueueLimit = 0,
            AutoReplenishment = true
        }));
    options.AddPolicy("api", context => RateLimitPartition.GetSlidingWindowLimiter(
        partitionKey: GetClientKey(context),
        factory: _ => new SlidingWindowRateLimiterOptions
        {
            PermitLimit = rateLimits.ApiPermitLimit,
            Window = TimeSpan.FromSeconds(rateLimits.ApiWindowSeconds),
            SegmentsPerWindow = 6,
            QueueLimit = 0,
            AutoReplenishment = true
        }));
});

var app = builder.Build();

var applyMigrations = app.Configuration.GetValue<bool>("Database:ApplyMigrations");
// Production deployments must never serve code against an older schema. EF Core
// migrations are idempotent, so shared-IIS restarts safely re-check the history table.
if (applyMigrations || app.Environment.IsProduction())
{
    await DatabaseInitializer.InitializeAsync(app.Services);
}

// Configure the HTTP request pipeline.
app.UseForwardedHeaders();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseResponseCompression();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseMiddleware<StaticAssetCacheMiddleware>();
app.UseRouting();

app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();
app.UseOutputCache();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapHealthChecks("/health")
    .DisableRateLimiting();

static string GetClientKey(HttpContext context)
{
    var subject = context.User.FindFirst("sub")?.Value;
    return !string.IsNullOrWhiteSpace(subject)
        ? $"user:{subject}"
        : $"ip:{context.Connection.RemoteIpAddress?.ToString() ?? "unknown"}";
}

app.Run();

public partial class Program;
