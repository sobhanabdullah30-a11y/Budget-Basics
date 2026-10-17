namespace BudgetBasics.Infrastructure.Persistence;

public sealed class SiteStatistic
{
    public int Id { get; set; }
    public long TotalVisits { get; set; }
    public DateTimeOffset UpdatedUtc { get; set; }
}
