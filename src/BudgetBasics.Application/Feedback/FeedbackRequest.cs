namespace BudgetBasics.Application.Feedback;

public sealed record FeedbackRequest(
    string Name,
    string Email,
    int Rating,
    string Comments,
    string? UserId);
