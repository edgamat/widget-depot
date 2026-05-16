using System.ComponentModel.DataAnnotations;

namespace WidgetDepot.Web.Features.Accounts.Profile;

public class AddressFormModel
{
    [MaxLength(100, ErrorMessage = "Recipient name must not exceed 100 characters.")]
    public string RecipientName { get; set; } = string.Empty;

    [MaxLength(100, ErrorMessage = "Street line 1 must not exceed 100 characters.")]
    public string StreetLine1 { get; set; } = string.Empty;

    [MaxLength(100, ErrorMessage = "Street line 2 must not exceed 100 characters.")]
    public string? StreetLine2 { get; set; }

    [MaxLength(100, ErrorMessage = "City must not exceed 100 characters.")]
    public string City { get; set; } = string.Empty;

    public string State { get; set; } = string.Empty;

    [RegularExpression(@"^\d{5}(-\d{4})?$", ErrorMessage = "ZIP code must be in 5-digit (12345) or ZIP+4 (12345-6789) format.")]
    public string ZipCode { get; set; } = string.Empty;

    public bool HasAnyValue() =>
        !string.IsNullOrWhiteSpace(RecipientName) ||
        !string.IsNullOrWhiteSpace(StreetLine1) ||
        !string.IsNullOrWhiteSpace(City) ||
        !string.IsNullOrWhiteSpace(State) ||
        !string.IsNullOrWhiteSpace(ZipCode);
}

public class ProfileFormModel
{
    [Required(ErrorMessage = "First name is required.")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required.")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email address is required.")]
    [EmailAddress(ErrorMessage = "Email address must be a valid email.")]
    public string Email { get; set; } = string.Empty;

    public AddressFormModel ShippingAddress { get; set; } = new();
    public AddressFormModel BillingAddress { get; set; } = new();
}

public record AddressResponse(string RecipientName, string StreetLine1, string? StreetLine2, string City, string State, string ZipCode);

public record ProfileResponse(string FirstName, string LastName, string Email, AddressResponse? ShippingAddress, AddressResponse? BillingAddress);

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