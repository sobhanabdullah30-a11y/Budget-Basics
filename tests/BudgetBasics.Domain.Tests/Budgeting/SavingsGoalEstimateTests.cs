using BudgetBasics.Domain.Budgeting;

namespace BudgetBasics.Domain.Tests.Budgeting;

public sealed class SavingsGoalEstimateTests
{
    [Fact]
    public void Create_PartiallyFundedGoal_CalculatesRemainingMonthsAndProgress()
    {
        var result = SavingsGoalEstimate.Create(1_000m, 250m, 200m);

        Assert.Equal(750m, result.RemainingAmount);
        Assert.Equal(4, result.EstimatedMonths);
        Assert.Equal(25m, result.ProgressPercent);
    }

    [Fact]
    public void Create_CompletedGoal_ClampsProgressAndReturnsZeroMonths()
    {
        var result = SavingsGoalEstimate.Create(1_000m, 1_200m, 100m);

        Assert.Equal(0m, result.RemainingAmount);
        Assert.Equal(0, result.EstimatedMonths);
        Assert.Equal(100m, result.ProgressPercent);
    }
}
