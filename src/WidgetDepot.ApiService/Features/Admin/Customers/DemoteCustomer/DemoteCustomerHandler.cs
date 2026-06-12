using Microsoft.EntityFrameworkCore;

using WidgetDepot.ApiService.Data;

namespace WidgetDepot.ApiService.Features.Admin.Customers.DemoteCustomer;

public record DemoteCustomerSuccess;

public abstract record DemoteCustomerError
{
    public record NotFound : DemoteCustomerError;
    public record CannotDemoteSelf : DemoteCustomerError;
}

public class DemoteCustomerHandler(AppDbContext db)
{
    public async Task<object> DemoteAsync(int customerId, int requestingAdminId, CancellationToken cancellationToken)
    {
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