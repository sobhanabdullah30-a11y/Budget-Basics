using BudgetBasics.Application.Planning;
using Microsoft.EntityFrameworkCore;

namespace BudgetBasics.Infrastructure.Persistence;

internal sealed class ExpensePlannerService(ApplicationDbContext dbContext) : IExpensePlannerService
{
    public async Task<IReadOnlyList<SavedExpenseItem>> GetAsync(
        string userId,
        CancellationToken cancellationToken = default) =>
        await dbContext.ExpenseEntries
            .AsNoTracking()
            .Where(expense => expense.UserId == userId)
            .OrderByDescending(expense => expense.ExpenseDate)
            .ThenByDescending(expense => expense.Id)
            .Select(expense => new SavedExpenseItem(
                expense.Id,
                expense.ExpenseDate,
                expense.Category,
                expense.Description,
                expense.Amount))
            .ToListAsync(cancellationToken);

    public async Task<SavedExpenseItem> SaveAsync(
        string userId,
        ExpenseDraft draft,
        CancellationToken cancellationToken = default)
    {
        ExpenseEntry entity;
        if (draft.Id is int id)
        {
            entity = await dbContext.ExpenseEntries
                .SingleOrDefaultAsync(
                    expense => expense.Id == id && expense.UserId == userId,
                    cancellationToken)
                ?? throw new InvalidOperationException("The expense was not found.");
        }
        else
        {
            entity = new ExpenseEntry
            {
                UserId = userId,
                Category = draft.Category,
                Description = draft.Description,
                CreatedUtc = DateTimeOffset.UtcNow
            };
            dbContext.ExpenseEntries.Add(entity);
        }

        entity.ExpenseDate = draft.Date;
        entity.Category = draft.Category;
        entity.Description = draft.Description;
        entity.Amount = draft.Amount;
        entity.UpdatedUtc = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return new SavedExpenseItem(
            entity.Id,
            entity.ExpenseDate,
            entity.Category,
            entity.Description,
            entity.Amount);
    }

    public async Task<bool> DeleteAsync(
        string userId,
        int id,
        CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.ExpenseEntries
            .SingleOrDefaultAsync(
                expense => expense.Id == id && expense.UserId == userId,
                cancellationToken);
        if (entity is null)
        {
            return false;
        }

        dbContext.ExpenseEntries.Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
