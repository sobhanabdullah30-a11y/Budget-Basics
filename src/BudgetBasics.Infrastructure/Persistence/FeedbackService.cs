using BudgetBasics.Application.Feedback;

namespace BudgetBasics.Infrastructure.Persistence;

internal sealed class FeedbackService(ApplicationDbContext dbContext) : IFeedbackService
{
    public async Task SubmitAsync(
        FeedbackRequest request,
        CancellationToken cancellationToken = default)
    {
        dbContext.FeedbackSubmissions.Add(new FeedbackSubmission
        {
            UserId = request.UserId,
            Name = request.Name.Trim(),
            Email = request.Email.Trim(),
            Rating = request.Rating,
            Comments = request.Comments.Trim(),
            SubmittedUtc = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
