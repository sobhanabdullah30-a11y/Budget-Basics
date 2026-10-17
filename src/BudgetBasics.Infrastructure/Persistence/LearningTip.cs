namespace BudgetBasics.Infrastructure.Persistence;

public sealed class LearningTip
{
    public int Id { get; set; }
    public string HabitKey { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
