using BudgetBasics.Domain.Budgeting;

namespace BudgetBasics.Domain.Tests.Budgeting;

public sealed class BudgetAllocationTests
{
    [Fact]
    public void Create_ValidIncome_ReturnsFiftyThirtyTwentySplit()
    {
        var result = BudgetAllocation.Create(1_000m);

        Assert.Equal(500m, result.Needs);
        Assert.Equal(300m, result.Wants);
        Assert.Equal(200m, result.Savings);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_NonPositiveIncome_Throws(decimal income)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => BudgetAllocation.Create(income));
    }
}
