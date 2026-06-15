using Microsoft.EntityFrameworkCore;

using WidgetDepot.ApiService.Data;
using WidgetDepot.ApiService.Shared;

namespace WidgetDepot.ApiService.Features.Admin.Customers.DemoteCustomer;

public record DemoteCustomerSuccess;

public abstract record DemoteCustomerError
{
    public record NotFound : DemoteCustomerError;
    public record CannotDemoteSelf : DemoteCustomerError;
}

public record DemoteCustomerCommand(int CustomerId, int RequestingAdminId) : IRequest<object>;

public class DemoteCustomerHandler(AppDbContext db) : IRequestHandler<DemoteCustomerCommand, object>
{
    public async Task<object> HandleAsync(DemoteCustomerCommand command, CancellationToken cancellationToken)
    {
        var customerId = command.CustomerId;
        var requestingAdminId = command.RequestingAdminId;

        if (customerId == requestingAdminId)
            return new DemoteCustomerError.CannotDemoteSelf();

        var customer = await db.Customers
            .SingleOrDefaultAsync(c => c.Id == customerId, cancellationToken);

        if (customer is null)
            return new DemoteCustomerError.NotFound();

        customer.IsAdmin = false;
        await db.SaveChangesAsync(cancellationToken);

        return new DemoteCustomerSuccess();
    }
}