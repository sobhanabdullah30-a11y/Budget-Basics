using BudgetBasics.Application.Common;
using Microsoft.EntityFrameworkCore;

namespace BudgetBasics.Infrastructure.Persistence;

internal sealed class VisitorCounterService(ApplicationDbContext dbContext) : IVisitorCounterService
{
    public async Task<long> RecordVisitAsync(CancellationToken cancellationToken = default)
    {
        var statistic = await dbContext.SiteStatistics
            .OrderBy(item => item.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (statistic is null)
        {
            statistic = new SiteStatistic
            {
                TotalVisits = 1,
                UpdatedUtc = DateTimeOffset.UtcNow
            };
            dbContext.SiteStatistics.Add(statistic);
        }
        else
        {
            statistic.TotalVisits++;
            statistic.UpdatedUtc = DateTimeOffset.UtcNow;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return statistic.TotalVisits;
    }
}
