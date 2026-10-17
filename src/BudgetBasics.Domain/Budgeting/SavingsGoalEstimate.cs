namespace BudgetBasics.Domain.Budgeting;

public sealed record SavingsGoalEstimate(decimal RemainingAmount, int EstimatedMonths, decimal ProgressPercent)
{
    public static SavingsGoalEstimate Create(
        decimal targetAmount,
        decimal currentSavings,
        decimal monthlyContribution)
    {
        if (targetAmount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(targetAmount), "Target amount must be greater than zero.");
        }

        if (currentSavings < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(currentSavings), "Current savings cannot be negative.");
        }

        if (monthlyContribution <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(monthlyContribution), "Monthly contribution must be greater than zero.");
        }

        var remaining = decimal.Max(0, targetAmount - currentSavings);
        var months = remaining == 0 ? 0 : (int)decimal.Ceiling(remaining / monthlyContribution);
        var progress = decimal.Min(100, decimal.Round(currentSavings / targetAmount * 100, 1));

        return new SavingsGoalEstimate(remaining, months, progress);
    }
}
