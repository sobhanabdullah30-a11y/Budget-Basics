namespace BudgetBasics.Application.Feedback;

public interface IFeedbackService
{
    Task SubmitAsync(FeedbackRequest request, CancellationToken cancellationToken = default);
}
