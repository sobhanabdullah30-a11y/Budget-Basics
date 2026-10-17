namespace BudgetBasics.Application.Common;

public interface IVisitorCounterService
{
    Task<long> RecordVisitAsync(CancellationToken cancellationToken = default);
}
