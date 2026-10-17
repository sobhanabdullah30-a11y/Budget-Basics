using BudgetBasics.Application.Learning;
using Microsoft.EntityFrameworkCore;

namespace BudgetBasics.Infrastructure.Persistence;

public sealed class LearningTipService(ApplicationDbContext dbContext) : ILearningTipService
{
    public async Task<IReadOnlyList<StudentTip>> GetByHabitAsync(
        string habitKey,
        CancellationToken cancellationToken = default)
    {
        var normalizedHabit = habitKey.Trim().ToLowerInvariant();

        return await dbContext.LearningTips
            .AsNoTracking()
            .Where(tip => tip.IsActive && (tip.HabitKey == "all" || tip.HabitKey == normalizedHabit))
            .OrderBy(tip => tip.SortOrder)
            .ThenBy(tip => tip.Id)
            .Select(tip => new StudentTip(tip.Id, tip.HabitKey, tip.Title, tip.Message))
            .ToListAsync(cancellationToken);
    }
}
