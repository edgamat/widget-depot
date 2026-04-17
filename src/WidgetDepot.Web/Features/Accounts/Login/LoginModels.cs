using System.ComponentModel.DataAnnotations;

namespace WidgetDepot.Web.Features.Accounts.Login;

public class LoginFormModel
{
    [Required(ErrorMessage = "Email address is required.")]
    [EmailAddress(ErrorMessage = "Email address must be a valid email.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; } = string.Empty;
}

public record LoginResponse(int CustomerId, string Email, string FirstName);

public abstract record LoginResult
{
    public record Success(LoginResponse Customer) : LoginResult;
    public record InvalidCredentials : LoginResult;
    public record Failure : LoginResult;
}