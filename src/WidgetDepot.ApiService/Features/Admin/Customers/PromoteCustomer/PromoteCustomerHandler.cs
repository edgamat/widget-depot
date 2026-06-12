using Microsoft.EntityFrameworkCore;

using WidgetDepot.ApiService.Data;

namespace WidgetDepot.ApiService.Features.Admin.Customers.PromoteCustomer;

public record PromoteCustomerSuccess;

public abstract record PromoteCustomerError
{
    public record NotFound : PromoteCustomerError;
}

public class PromoteCustomerHandler(AppDbContext db)
{
    public async Task<object> PromoteAsync(int customerId, CancellationToken cancellationToken)
    {
        var customer = await db.Customers
            .SingleOrDefaultAsync(c => c.Id == customerId, cancellationToken);

        if (customer is null)
            return new PromoteCustomerError.NotFound();

        customer.IsAdmin = true;
        await db.SaveChangesAsync(cancellationToken);

        return new PromoteCustomerSuccess();
    }
}