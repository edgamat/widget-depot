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