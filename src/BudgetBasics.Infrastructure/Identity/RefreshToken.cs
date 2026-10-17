namespace BudgetBasics.Infrastructure.Identity;

public sealed class RefreshToken
{
    public Guid Id { get; set; }
    public required string UserId { get; set; }
    public required string TokenHash { get; set; }
    public required string FamilyId { get; set; }
    public DateTimeOffset CreatedUtc { get; set; }
    public DateTimeOffset ExpiresUtc { get; set; }
    public DateTimeOffset? RevokedUtc { get; set; }
    public string? ReplacedByTokenHash { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public bool IsActive(DateTimeOffset now) => RevokedUtc is null && ExpiresUtc > now;
}
