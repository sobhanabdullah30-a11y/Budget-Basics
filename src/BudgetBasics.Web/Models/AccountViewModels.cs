using System.ComponentModel.DataAnnotations;

namespace BudgetBasics.Web.Models;

public sealed class RegisterViewModel
{
    [Required, StringLength(80, MinimumLength = 2)]
    [Display(Name = "Your name")]
    public string DisplayName { get; init; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required, DataType(DataType.Password), StringLength(100, MinimumLength = 12)]
    public string Password { get; init; } = string.Empty;

    [Required, DataType(DataType.Password), Compare(nameof(Password))]
    [Display(Name = "Confirm password")]
    public string ConfirmPassword { get; init; } = string.Empty;
}

public sealed class LoginViewModel
{
    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required, DataType(DataType.Password)]
    public string Password { get; init; } = string.Empty;
}
