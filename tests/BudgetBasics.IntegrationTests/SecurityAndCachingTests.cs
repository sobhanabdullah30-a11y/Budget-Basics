using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;

namespace BudgetBasics.IntegrationTests;

public sealed class SecurityAndCachingTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public SecurityAndCachingTests(WebApplicationFactory<Program> factory)
    {
        Environment.SetEnvironmentVariable(
            "Jwt__SigningKey",
            "integration-tests-only-signing-key-with-at-least-sixty-four-characters-1234567890");
        Environment.SetEnvironmentVariable("Database__ApplyMigrations", "false");
        _client = factory
            .WithWebHostBuilder(builder => builder.ConfigureLogging(logging => logging.ClearProviders()))
            .CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
    }

    [Fact]
    public async Task PublicPage_SendsDefenseInDepthSecurityHeaders()
    {
        var response = await _client.GetAsync("/Home/About");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("nosniff", GetHeader(response, "X-Content-Type-Options"));
        Assert.Equal("DENY", GetHeader(response, "X-Frame-Options"));
        Assert.Equal("same-origin", GetHeader(response, "Cross-Origin-Opener-Policy"));
        Assert.Contains("frame-ancestors 'none'", GetHeader(response, "Content-Security-Policy"));
    }

    [Fact]
    public async Task AccountPage_DisablesBrowserAndSharedCaching()
    {
        var response = await _client.GetAsync("/Account/Login");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("no-store", GetHeader(response, "Cache-Control"));
    }

    [Fact]
    public async Task StaticAsset_UsesVersionedUrlAndImmutableCachePolicy()
    {
        var page = await _client.GetAsync("/Home/About");
        var html = await page.Content.ReadAsStringAsync();
        var asset = await _client.GetAsync("/css/site.css");

        Assert.Contains("/css/site.css?v=", html);
        Assert.Equal(HttpStatusCode.OK, asset.StatusCode);
        Assert.Contains("immutable", GetHeader(asset, "Cache-Control"));
        Assert.Contains("max-age=31536000", GetHeader(asset, "Cache-Control"));
    }

    private static string GetHeader(HttpResponseMessage response, string name) =>
        string.Join(",", response.Headers.GetValues(name));
}
