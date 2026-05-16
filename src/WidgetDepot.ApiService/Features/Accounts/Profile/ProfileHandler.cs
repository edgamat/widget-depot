using Microsoft.EntityFrameworkCore;

using WidgetDepot.ApiService.Data;

namespace WidgetDepot.ApiService.Features.Accounts.Profile;

public record AddressDto(
    string RecipientName,
    string StreetLine1,
    string? StreetLine2,
    string City,
    string State,
    string ZipCode);

public record GetProfileResponse(string FirstName, string LastName, string Email, AddressDto? ShippingAddress, AddressDto? BillingAddress);

public record UpdateProfileRequest(string FirstName, string LastName, string Email, AddressDto? ShippingAddress, AddressDto? BillingAddress);

public record UpdateProfileResponse(string FirstName, string LastName, string Email, AddressDto? ShippingAddress, AddressDto? BillingAddress);

public abstract record ProfileError
{
    public record NotFound : ProfileError;
    public record EmailAlreadyRegistered : ProfileError;
}

public class ProfileHandler(AppDbContext db)
{
    public async Task<object> GetAsync(int customerId, CancellationToken cancellationToken)
    {
        var customer = await db.Customers
            .SingleOrDefaultAsync(c => c.Id == customerId, cancellationToken);

        if (customer is null)
            return new ProfileError.NotFound();

        return new GetProfileResponse(
            customer.FirstName,
            customer.LastName,
            customer.Email,
            MapAddress(customer.ShippingAddress),
            MapAddress(customer.BillingAddress));
    }

    public async Task<object> UpdateAsync(int customerId, UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        var customer = await db.Customers
            .SingleOrDefaultAsync(c => c.Id == customerId, cancellationToken);

        if (customer is null)
            return new ProfileError.NotFound();

        var emailTaken = await db.Customers
            .AnyAsync(c => c.Email == request.Email && c.Id != customerId, cancellationToken);

        if (emailTaken)
            return new ProfileError.EmailAlreadyRegistered();

        customer.FirstName = request.FirstName;
        customer.LastName = request.LastName;
        customer.Email = request.Email;
        customer.ShippingAddress = request.ShippingAddress is not null ? MapAddress(request.ShippingAddress) : null;
        customer.BillingAddress = request.BillingAddress is not null ? MapAddress(request.BillingAddress) : null;

        await db.SaveChangesAsync(cancellationToken);

        return new UpdateProfileResponse(
            customer.FirstName,
            customer.LastName,
            customer.Email,
            MapAddress(customer.ShippingAddress),
            MapAddress(customer.BillingAddress));
    }

    private static AddressDto? MapAddress(Address? address)
    {
        if (address is null)
            return null;

        return new AddressDto(
            address.RecipientName,
            address.StreetLine1,
            address.StreetLine2,
            address.City,
            address.State,
            address.ZipCode);
    }

    private static Address MapAddress(AddressDto dto)
    {
        return new Address
        {
            RecipientName = dto.RecipientName,
            StreetLine1 = dto.StreetLine1,
            StreetLine2 = dto.StreetLine2,
            City = dto.City,
            State = dto.State,
            ZipCode = dto.ZipCode
        };
    }
}