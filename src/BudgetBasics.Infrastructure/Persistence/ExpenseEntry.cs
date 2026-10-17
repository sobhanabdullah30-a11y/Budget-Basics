using BudgetBasics.Infrastructure.Identity;

namespace BudgetBasics.Infrastructure.Persistence;

public sealed class ExpenseEntry
{
    public int Id { get; set; }
    public required string UserId { get; set; }
    public DateOnly ExpenseDate { get; set; }
    public required string Category { get; set; }
    public required string Description { get; set; }
    public decimal Amount { get; set; }
    public DateTimeOffset CreatedUtc { get; set; }
    public DateTimeOffset UpdatedUtc { get; set; }
    public ApplicationUser User { get; set; } = null!;
}
