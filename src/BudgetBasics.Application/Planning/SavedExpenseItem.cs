namespace BudgetBasics.Application.Planning;

public sealed record SavedExpenseItem(
    int Id,
    DateOnly Date,
    string Category,
    string Description,
    decimal Amount);
