using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;

namespace BudgetBasics.IntegrationTests;

public sealed class HealthEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public HealthEndpointTests(WebApplicationFactory<Program> factory)
    {
        Environment.SetEnvironmentVariable(
            "Jwt__SigningKey",
            "integration-tests-only-signing-key-with-at-least-sixty-four-characters-1234567890");
        Environment.SetEnvironmentVariable("Database__ApplyMigrations", "false");
        _client = factory
            .WithWebHostBuilder(builder => builder.ConfigureLogging(logging => logging.ClearProviders()))
            .CreateClient();
    }

    [Fact]
    public async Task Health_ReturnsOk()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Home_IncludesInteractiveLearningFeatures()
    {
        var response = await _client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Question 1 of 6", html);
        Assert.Contains("Get a tip for your spending habit", html);
    }

    [Fact]
    public async Task StudentTip_RejectsUnknownHabitBeforeDatabaseQuery()
    {
        var response = await _client.GetAsync("/tools/student-tip?habit=unknown");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
