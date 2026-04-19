using System.ComponentModel.DataAnnotations;

namespace WidgetDepot.Web.Features.Accounts.PasswordChange;

public class PasswordChangeFormModel
{
    [Required(ErrorMessage = "Current password is required.")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "New password is required.")]
    [MinLength(8, ErrorMessage = "New password must be at least 8 characters.")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirm new password is required.")]
    [Compare(nameof(NewPassword), ErrorMessage = "New password and confirmation do not match.")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}

public abstract record ChangePasswordResult
{
    public record Success : ChangePasswordResult;
    public record IncorrectPassword : ChangePasswordResult;
    public record Failure : ChangePasswordResult;
}