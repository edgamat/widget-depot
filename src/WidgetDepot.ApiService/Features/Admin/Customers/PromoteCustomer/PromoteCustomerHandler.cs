using Microsoft.EntityFrameworkCore;

using WidgetDepot.ApiService.Data;
using WidgetDepot.ApiService.Shared;

namespace WidgetDepot.ApiService.Features.Admin.Customers.PromoteCustomer;

public record PromoteCustomerSuccess;

public abstract record PromoteCustomerError
{
    public record NotFound : PromoteCustomerError;
}

public record PromoteCustomerCommand(int CustomerId) : IRequest<object>;

public class PromoteCustomerHandler(AppDbContext db) : IRequestHandler<PromoteCustomerCommand, object>
{
    public async Task<object> HandleAsync(PromoteCustomerCommand command, CancellationToken cancellationToken)
    {
        var customerId = command.CustomerId;

        var customer = await db.Customers
            .SingleOrDefaultAsync(c => c.Id == customerId, cancellationToken);

        if (customer is null)
            return new PromoteCustomerError.NotFound();

        customer.IsAdmin = true;
        await db.SaveChangesAsync(cancellationToken);

        return new PromoteCustomerSuccess();
    }
}