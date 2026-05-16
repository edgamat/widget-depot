using System.ComponentModel.DataAnnotations;

namespace WidgetDepot.Web.Features.Orders.Create.Step2;

public class Step2FormModel
{
    [Required(ErrorMessage = "Recipient name is required.")]
    [MaxLength(100, ErrorMessage = "Recipient name must not exceed 100 characters.")]
    public string ShippingRecipientName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Street line 1 is required.")]
    [MaxLength(100, ErrorMessage = "Street line 1 must not exceed 100 characters.")]
    public string ShippingStreetLine1 { get; set; } = string.Empty;

    [MaxLength(100, ErrorMessage = "Street line 2 must not exceed 100 characters.")]
    public string? ShippingStreetLine2 { get; set; }

    [Required(ErrorMessage = "City is required.")]
    [MaxLength(100, ErrorMessage = "City must not exceed 100 characters.")]
    public string ShippingCity { get; set; } = string.Empty;

    [Required(ErrorMessage = "State is required.")]
    public string ShippingState { get; set; } = string.Empty;

    [Required(ErrorMessage = "ZIP code is required.")]
    [RegularExpression(@"^\d{5}(-\d{4})?$", ErrorMessage = "ZIP code must be in 5-digit (12345) or ZIP+4 (12345-6789) format.")]
    public string ShippingZipCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Recipient name is required.")]
    [MaxLength(100, ErrorMessage = "Recipient name must not exceed 100 characters.")]
    public string BillingRecipientName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Street line 1 is required.")]
    [MaxLength(100, ErrorMessage = "Street line 1 must not exceed 100 characters.")]
    public string BillingStreetLine1 { get; set; } = string.Empty;

    [MaxLength(100, ErrorMessage = "Street line 2 must not exceed 100 characters.")]
    public string? BillingStreetLine2 { get; set; }

    [Required(ErrorMessage = "City is required.")]
    [MaxLength(100, ErrorMessage = "City must not exceed 100 characters.")]
    public string BillingCity { get; set; } = string.Empty;

    [Required(ErrorMessage = "State is required.")]
    public string BillingState { get; set; } = string.Empty;

    [Required(ErrorMessage = "ZIP code is required.")]
    [RegularExpression(@"^\d{5}(-\d{4})?$", ErrorMessage = "ZIP code must be in 5-digit (12345) or ZIP+4 (12345-6789) format.")]
    public string BillingZipCode { get; set; } = string.Empty;
}

public static class UsStates
{
    public static readonly IReadOnlyList<(string Code, string Name)> All =
    [
        ("AL", "Alabama"),
        ("AK", "Alaska"),
        ("AZ", "Arizona"),
        ("AR", "Arkansas"),
        ("CA", "California"),
        ("CO", "Colorado"),
        ("CT", "Connecticut"),
        ("DE", "Delaware"),
        ("DC", "District of Columbia"),
        ("FL", "Florida"),
        ("GA", "Georgia"),
        ("HI", "Hawaii"),
        ("ID", "Idaho"),
        ("IL", "Illinois"),
        ("IN", "Indiana"),
        ("IA", "Iowa"),
        ("KS", "Kansas"),
        ("KY", "Kentucky"),
        ("LA", "Louisiana"),
        ("ME", "Maine"),
        ("MD", "Maryland"),
        ("MA", "Massachusetts"),
        ("MI", "Michigan"),
        ("MN", "Minnesota"),
        ("MS", "Mississippi"),
        ("MO", "Missouri"),
        ("MT", "Montana"),
        ("NE", "Nebraska"),
        ("NV", "Nevada"),
        ("NH", "New Hampshire"),
        ("NJ", "New Jersey"),
        ("NM", "New Mexico"),
        ("NY", "New York"),
        ("NC", "North Carolina"),
        ("ND", "North Dakota"),
        ("OH", "Ohio"),
        ("OK", "Oklahoma"),
        ("OR", "Oregon"),
        ("PA", "Pennsylvania"),
        ("RI", "Rhode Island"),
        ("SC", "South Carolina"),
        ("SD", "South Dakota"),
        ("TN", "Tennessee"),
        ("TX", "Texas"),
        ("UT", "Utah"),
        ("VT", "Vermont"),
        ("VA", "Virginia"),
        ("WA", "Washington"),
        ("WV", "West Virginia"),
        ("WI", "Wisconsin"),
        ("WY", "Wyoming"),
    ];
}

public record GetDraftOrderItemResponse(int WidgetId, int Quantity);

public record GetDraftOrderAddressResponse(
    string RecipientName,
    string StreetLine1,
    string? StreetLine2,
    string City,
    string State,
    string ZipCode);

public record GetDraftOrderResponse(
    int Id,
    string Status,
    IReadOnlyList<GetDraftOrderItemResponse> Items,
    GetDraftOrderAddressResponse? ShippingAddress,
    GetDraftOrderAddressResponse? BillingAddress);

public record AddressRequest(
    string RecipientName,
    string StreetLine1,
    string? StreetLine2,
    string City,
    string State,
    string ZipCode);

public record SaveAddressesRequest(AddressRequest ShippingAddress, AddressRequest BillingAddress);

public record ProfileAddressData(
    string RecipientName,
    string StreetLine1,
    string? StreetLine2,
    string City,
    string State,
    string ZipCode);

public record ProfileAddressesResponse(
    ProfileAddressData? ShippingAddress,
    ProfileAddressData? BillingAddress);

public abstract record SaveAddressesResult
{
    public record Success : SaveAddressesResult;
    public record NotFound : SaveAddressesResult;
    public record Forbidden : SaveAddressesResult;
    public record Failure : SaveAddressesResult;
}