namespace BudgetBasics.Application.Learning;

public interface ILearningTipService
{
    Task<IReadOnlyList<StudentTip>> GetByHabitAsync(
        string habitKey,
        CancellationToken cancellationToken = default);
}
