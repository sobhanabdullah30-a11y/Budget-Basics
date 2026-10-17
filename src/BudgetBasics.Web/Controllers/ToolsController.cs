using System.Security.Claims;
using BudgetBasics.Application.Assistant;
using BudgetBasics.Application.Common;
using BudgetBasics.Application.Feedback;
using BudgetBasics.Application.Planning;
using BudgetBasics.Application.Learning;
using BudgetBasics.Domain.Budgeting;
using BudgetBasics.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BudgetBasics.Web.Controllers;

[Route("tools")]
public sealed class ToolsController(
    IExpensePlannerService expensePlannerService,
    IFeedbackService feedbackService,
    IBudgetBeeService budgetBeeService,
    ILearningTipService learningTipService) : Controller
{
    private static readonly HashSet<string> SupportedHabits =
        ["all", "food", "transport", "impulse", "subscriptions", "saving"];

    [HttpPost("budget-split")]
    [EnableRateLimiting("api")]
    public IActionResult BudgetSplit(BudgetSplitInput input)
    {
        if (!ModelState.IsValid || input.MonthlyIncome is null)
        {
            return BadRequest(new { message = "Enter a valid monthly income greater than zero." });
        }

        var result = BudgetAllocation.Create(input.MonthlyIncome.Value);
        return Json(new { result.Income, result.Needs, result.Wants, result.Savings });
    }

    [HttpPost("savings-goal")]
    [EnableRateLimiting("api")]
    public IActionResult SavingsGoal(SavingsGoalInput input)
    {
        if (!ModelState.IsValid || input.TargetAmount is null || input.CurrentSavings is null || input.MonthlyContribution is null)
        {
            return BadRequest(new { message = "Complete every field using valid positive amounts." });
        }

        var result = SavingsGoalEstimate.Create(
            input.TargetAmount.Value,
            input.CurrentSavings.Value,
            input.MonthlyContribution.Value);
        return Json(new
        {
            input.GoalName,
            result.RemainingAmount,
            result.EstimatedMonths,
            result.ProgressPercent
        });
    }

    [HttpPost("chat")]
    [EnableRateLimiting("assistant")]
    public async Task<IActionResult> Chat(
        ChatInput input,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { message = "Please enter a short budgeting question." });
        }

        var response = await budgetBeeService.AnswerAsync(input.Question, cancellationToken);
        return Json(new { response.Answer, response.Topic, response.Suggestions });
    }

    [HttpGet("student-tip")]
    [EnableRateLimiting("api")]
    [Microsoft.AspNetCore.OutputCaching.OutputCache(PolicyName = "reference-data")]
    public async Task<IActionResult> StudentTip(
        [FromQuery] string? habit,
        CancellationToken cancellationToken)
    {
        var normalizedHabit = (habit ?? "all").Trim().ToLowerInvariant();
        if (!SupportedHabits.Contains(normalizedHabit))
        {
            return BadRequest(new { message = "Choose a valid spending habit." });
        }

        var tips = await learningTipService.GetByHabitAsync(normalizedHabit, cancellationToken);
        if (tips.Count == 0)
        {
            return NotFound(new { message = "No learning tip is available right now." });
        }

        var specificTips = tips.Where(tip => tip.HabitKey == normalizedHabit).ToArray();
        var availableTips = specificTips.Length > 0 ? specificTips : tips.ToArray();
        var selected = availableTips[Random.Shared.Next(availableTips.Length)];
        return Json(new { selected.Title, selected.Message });
    }

    [Authorize(Policy = SecurityPolicies.AuthenticatedUser)]
    [HttpGet("expenses")]
    [EnableRateLimiting("api")]
    public async Task<IActionResult> Expenses(CancellationToken cancellationToken)
    {
        var userId = RequireUserId();
        var expenses = await expensePlannerService.GetAsync(userId, cancellationToken);
        return Json(expenses);
    }

    [Authorize(Policy = SecurityPolicies.AuthenticatedUser)]
    [HttpPost("expenses/save")]
    [EnableRateLimiting("api")]
    public async Task<IActionResult> SaveExpense(
        ExpenseInput input,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid || input.Date is null || input.Amount is null)
        {
            return BadRequest(new { message = "Complete the expense using valid values." });
        }

        try
        {
            var saved = await expensePlannerService.SaveAsync(
                RequireUserId(),
                new ExpenseDraft(
                    input.Id,
                    input.Date.Value,
                    input.Category,
                    input.Description.Trim(),
                    input.Amount.Value),
                cancellationToken);
            return Json(saved);
        }
        catch (InvalidOperationException)
        {
            return NotFound(new { message = "The expense could not be found." });
        }
    }

    [Authorize(Policy = SecurityPolicies.AuthenticatedUser)]
    [HttpPost("expenses/delete")]
    [EnableRateLimiting("api")]
    public async Task<IActionResult> DeleteExpense(
        DeleteExpenseInput input,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { message = "Select a valid expense." });
        }

        var removed = await expensePlannerService.DeleteAsync(
            RequireUserId(),
            input.Id,
            cancellationToken);
        return removed ? NoContent() : NotFound(new { message = "The expense could not be found." });
    }

    [HttpPost("feedback")]
    [EnableRateLimiting("api")]
    public async Task<IActionResult> Feedback(
        FeedbackInput input,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid || input.Rating is null)
        {
            return BadRequest(new { message = "Complete every feedback field using valid information." });
        }

        await feedbackService.SubmitAsync(
            new FeedbackRequest(
                input.Name,
                input.Email,
                input.Rating.Value,
                input.Comments,
                User.FindFirstValue("sub")),
            cancellationToken);
        return Json(new { message = "Thank you. Your feedback was saved successfully." });
    }

    private string RequireUserId() =>
        User.FindFirstValue("sub")
        ?? throw new InvalidOperationException("The authenticated user identifier is missing.");
}
