namespace BudgetBasics.Application.Planning;

public sealed record ExpenseDraft(
    int? Id,
    DateOnly Date,
    string Category,
    string Description,
    decimal Amount);
