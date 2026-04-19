using System.ComponentModel.DataAnnotations;

namespace WidgetDepot.Web.Features.Accounts.Profile;

public class ProfileFormModel
{
    [Required(ErrorMessage = "First name is required.")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required.")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email address is required.")]
    [EmailAddress(ErrorMessage = "Email address must be a valid email.")]
    public string Email { get; set; } = string.Empty;
}

public record ProfileResponse(string FirstName, string LastName, string Email);

public abstract record LoadProfileResult
{
    public record Success(ProfileResponse Profile) : LoadProfileResult;
    public record Failure : LoadProfileResult;
}

public abstract record UpdateProfileResult
{
    public record Success(ProfileResponse Profile) : UpdateProfileResult;
    public record DuplicateEmail : UpdateProfileResult;
    public record Failure : UpdateProfileResult;
}