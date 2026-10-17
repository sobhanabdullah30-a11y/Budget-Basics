namespace BudgetBasics.Application.Authentication;

public sealed record AuthenticationResult(
    bool Succeeded,
    string Message,
    string? DisplayName = null,
    string? AccessToken = null,
    string? RefreshToken = null,
    DateTimeOffset? AccessTokenExpiresUtc = null,
    DateTimeOffset? RefreshTokenExpiresUtc = null);
