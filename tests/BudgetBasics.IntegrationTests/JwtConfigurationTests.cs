using BudgetBasics.Infrastructure;
using BudgetBasics.Infrastructure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BudgetBasics.IntegrationTests;

public sealed class JwtConfigurationTests
{
    [Fact]
    public void AddInfrastructure_BindsJwtOptionsForTokenIssuingService()
    {
        const string signingKey =
            "integration-tests-only-signing-key-with-at-least-sixty-four-characters-1234567890";

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Server=(localdb)\\mssqllocaldb;Database=UnusedTestDb;Trusted_Connection=True;",
                ["Jwt:Issuer"] = "BudgetBasics.Tests",
                ["Jwt:Audience"] = "BudgetBasics.Tests.Mvc",
                ["Jwt:SigningKey"] = signingKey,
                ["Jwt:AccessTokenMinutes"] = "15",
                ["Jwt:RefreshTokenDays"] = "7"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddInfrastructure(configuration);

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<JwtOptions>>().Value;

        Assert.Equal(signingKey, options.SigningKey);
        Assert.Equal("BudgetBasics.Tests", options.Issuer);
        Assert.Equal("BudgetBasics.Tests.Mvc", options.Audience);
    }
}
