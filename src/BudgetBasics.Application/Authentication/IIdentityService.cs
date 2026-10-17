namespace BudgetBasics.Application.Authentication;

public interface IIdentityService
{
    Task<bool> IsInRoleAsync(string userId, string role, CancellationToken cancellationToken = default);
}
