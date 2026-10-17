using BudgetBasics.Infrastructure.Identity;

namespace BudgetBasics.Infrastructure.Persistence;

public sealed class FeedbackSubmission
{
    public int Id { get; set; }
    public string? UserId { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public int Rating { get; set; }
    public required string Comments { get; set; }
    public DateTimeOffset SubmittedUtc { get; set; }
    public ApplicationUser? User { get; set; }
}
