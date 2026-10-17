namespace BudgetBasics.Application.Planning;

public interface IExpensePlannerService
{
    Task<IReadOnlyList<SavedExpenseItem>> GetAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task<SavedExpenseItem> SaveAsync(
        string userId,
        ExpenseDraft draft,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        string userId,
        int id,
        CancellationToken cancellationToken = default);
}
