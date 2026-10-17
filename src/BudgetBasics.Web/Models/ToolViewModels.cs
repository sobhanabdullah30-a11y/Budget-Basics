using System.ComponentModel.DataAnnotations;

namespace BudgetBasics.Web.Models;

public sealed class BudgetSplitInput
{
    [Required, Range(0.01, 1_000_000_000)]
    public decimal? MonthlyIncome { get; init; }
}

public sealed class SavingsGoalInput
{
    [Required, StringLength(80)]
    public string GoalName { get; init; } = string.Empty;

    [Required, Range(0.01, 1_000_000_000)]
    public decimal? TargetAmount { get; init; }

    [Required, Range(0, 1_000_000_000)]
    public decimal? CurrentSavings { get; init; }

    [Required, Range(0.01, 1_000_000_000)]
    public decimal? MonthlyContribution { get; init; }
}

public sealed class ChatInput
{
    [Required, StringLength(300)]
    public string Question { get; init; } = string.Empty;
}

public sealed class ExpenseInput
{
    public int? Id { get; init; }

    [Required]
    public DateOnly? Date { get; init; }

    [Required, RegularExpression("^(Food|Transport|Education|Entertainment|Shopping|Utilities|Miscellaneous)$")]
    public string Category { get; init; } = string.Empty;

    [Required, StringLength(80, MinimumLength = 2)]
    public string Description { get; init; } = string.Empty;

    [Required, Range(0.01, 1_000_000_000)]
    public decimal? Amount { get; init; }
}

public sealed class DeleteExpenseInput
{
    [Range(1, int.MaxValue)]
    public int Id { get; init; }
}

public sealed class FeedbackInput
{
    [Required, StringLength(80, MinimumLength = 2)]
    public string Name { get; init; } = string.Empty;

    [Required, EmailAddress, StringLength(254)]
    public string Email { get; init; } = string.Empty;

    [Required, Range(1, 5)]
    public int? Rating { get; init; }

    [Required, StringLength(500, MinimumLength = 5)]
    public string Comments { get; init; } = string.Empty;
}
