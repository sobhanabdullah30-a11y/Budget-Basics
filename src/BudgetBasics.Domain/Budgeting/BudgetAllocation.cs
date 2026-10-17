namespace BudgetBasics.Domain.Budgeting;

public sealed record BudgetAllocation(decimal Income, decimal Needs, decimal Wants, decimal Savings)
{
    public static BudgetAllocation Create(decimal monthlyIncome)
    {
        if (monthlyIncome <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(monthlyIncome), "Monthly income must be greater than zero.");
        }

        return new BudgetAllocation(
            monthlyIncome,
            decimal.Round(monthlyIncome * 0.50m, 2),
            decimal.Round(monthlyIncome * 0.30m, 2),
            decimal.Round(monthlyIncome * 0.20m, 2));
    }
}
