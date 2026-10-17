namespace BudgetBasics.Application.Assistant;

public interface IBudgetBeeService
{
    Task<BudgetBeeResponse> AnswerAsync(
        string question,
        CancellationToken cancellationToken = default);
}

public sealed record BudgetBeeResponse(
    string Answer,
    string Topic,
    IReadOnlyList<string> Suggestions);
