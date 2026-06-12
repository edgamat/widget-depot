using Microsoft.EntityFrameworkCore;

using WidgetDepot.ApiService.Data;

namespace WidgetDepot.ApiService.Features.Admin.Customers.UpdateCustomerEmail;

public record UpdateCustomerEmailRequest(string Email);

public record UpdateCustomerEmailSuccess;

public abstract record UpdateCustomerEmailError
{
    public record NotFound : UpdateCustomerEmailError;
    public record EmailAlreadyInUse : UpdateCustomerEmailError;
}

public class UpdateCustomerEmailHandler(AppDbContext db)
{
    public async Task<object> UpdateAsync(int customerId, UpdateCustomerEmailRequest request, CancellationToken cancellationToken)
    {
        var customer = await db.Customers
            .SingleOrDefaultAsync(c => c.Id == customerId, cancellationToken);

        if (customer is null)
            return new UpdateCustomerEmailError.NotFound();

        var emailTaken = await db.Customers
            .AnyAsync(c => c.Email == request.Email && c.Id != customerId, cancellationToken);

        if (emailTaken)
            return new UpdateCustomerEmailError.EmailAlreadyInUse();

        customer.Email = request.Email;
        await db.SaveChangesAsync(cancellationToken);

        return new UpdateCustomerEmailSuccess();
    }
}