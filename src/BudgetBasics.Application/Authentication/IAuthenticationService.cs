namespace BudgetBasics.Application.Authentication;

public interface IAuthenticationService
{
    Task<AuthenticationResult> RegisterAsync(
        string displayName,
        string email,
        string password,
        CancellationToken cancellationToken = default);

    Task<AuthenticationResult> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);

    Task<AuthenticationResult> RefreshAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);

    Task RevokeAsync(string refreshToken, CancellationToken cancellationToken = default);
}
