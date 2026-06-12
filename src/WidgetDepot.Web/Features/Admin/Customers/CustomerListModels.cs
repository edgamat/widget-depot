using System.ComponentModel.DataAnnotations;

namespace WidgetDepot.Web.Features.Admin.Customers;

public record CustomerListItem(int Id, string FirstName, string LastName, string Email, bool IsAdmin);

public record GetCustomersResponse(IReadOnlyList<CustomerListItem> Customers, int TotalCount, int Page, int PageSize);

public record CustomerAddressDto(
    string RecipientName,
    string StreetLine1,
    string? StreetLine2,
    string City,
    string State,
    string ZipCode);

public record CustomerProfileDto(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    bool IsAdmin,
    DateTime CreatedAt,
    CustomerAddressDto? ShippingAddress,
    CustomerAddressDto? BillingAddress);

public class EmailFormModel
{
    [Required(ErrorMessage = "Email address is required.")]
    [EmailAddress(ErrorMessage = "Email address must be a valid email.")]
    public string Email { get; set; } = string.Empty;
}

public abstract record UpdateEmailResult
{
    public record Success : UpdateEmailResult;
    public record EmailAlreadyInUse : UpdateEmailResult;
    public record Failure : UpdateEmailResult;
}

public abstract record ResetPasswordResult
{
    public record Success(string TemporaryPassword) : ResetPasswordResult;
    public record Failure : ResetPasswordResult;
}

public abstract record PromoteResult
{
    public record Success : PromoteResult;
    public record Failure : PromoteResult;
}

public abstract record DemoteResult
{
    public record Success : DemoteResult;
    public record Failure : DemoteResult;
}