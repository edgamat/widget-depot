using Microsoft.EntityFrameworkCore;

using WidgetDepot.ApiService.Data;
using WidgetDepot.ApiService.Shared;

namespace WidgetDepot.ApiService.Features.Admin.Customers.UpdateCustomerEmail;

public record UpdateCustomerEmailRequest(string Email);

public record UpdateCustomerEmailSuccess;

public abstract record UpdateCustomerEmailError
{
    public record NotFound : UpdateCustomerEmailError;
    public record EmailAlreadyInUse : UpdateCustomerEmailError;
}

public record UpdateCustomerEmailCommand(int CustomerId, UpdateCustomerEmailRequest Request) : IRequest<object>;

public class UpdateCustomerEmailHandler(AppDbContext db) : IRequestHandler<UpdateCustomerEmailCommand, object>
{
    public async Task<object> HandleAsync(UpdateCustomerEmailCommand command, CancellationToken cancellationToken)
    {
        var customerId = command.CustomerId;
        var request = command.Request;

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