using Microsoft.EntityFrameworkCore;

using WidgetDepot.ApiService.Data;

namespace WidgetDepot.ApiService.Features.Accounts.Profile;

public record GetProfileResponse(string FirstName, string LastName, string Email);

public record UpdateProfileRequest(string FirstName, string LastName, string Email);

public record UpdateProfileResponse(string FirstName, string LastName, string Email);

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

        return new GetProfileResponse(customer.FirstName, customer.LastName, customer.Email);
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

        await db.SaveChangesAsync(cancellationToken);

        return new UpdateProfileResponse(customer.FirstName, customer.LastName, customer.Email);
    }
}