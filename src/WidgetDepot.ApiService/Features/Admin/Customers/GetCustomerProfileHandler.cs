using Microsoft.EntityFrameworkCore;

using WidgetDepot.ApiService.Data;

namespace WidgetDepot.ApiService.Features.Admin.Customers;

public record CustomerAddressDto(
    string RecipientName,
    string StreetLine1,
    string? StreetLine2,
    string City,
    string State,
    string ZipCode);

public record CustomerProfileResponse(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    bool IsAdmin,
    DateTime CreatedAt,
    CustomerAddressDto? ShippingAddress,
    CustomerAddressDto? BillingAddress);

public abstract record CustomerProfileError
{
    public record NotFound : CustomerProfileError;
}

public class GetCustomerProfileHandler(AppDbContext db)
{
    public async Task<object> GetAsync(int customerId, CancellationToken cancellationToken)
    {
        var customer = await db.Customers
            .SingleOrDefaultAsync(c => c.Id == customerId, cancellationToken);

        if (customer is null)
            return new CustomerProfileError.NotFound();

        return new CustomerProfileResponse(
            customer.Id,
            customer.FirstName,
            customer.LastName,
            customer.Email,
            customer.IsAdmin,
            customer.CreatedAt,
            MapAddress(customer.ShippingAddress),
            MapAddress(customer.BillingAddress));
    }

    private static CustomerAddressDto? MapAddress(Address? address)
    {
        if (address is null)
            return null;

        return new CustomerAddressDto(
            address.RecipientName,
            address.StreetLine1,
            address.StreetLine2,
            address.City,
            address.State,
            address.ZipCode);
    }
}