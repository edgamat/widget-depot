using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using WidgetDepot.ApiService.Data;
using WidgetDepot.ApiService.Shared;

namespace WidgetDepot.ApiService.Features.Accounts.ForcePasswordChange;

public record ForcePasswordChangeRequest(string NewPassword);

public record ForcePasswordChangeSuccess;

public abstract record ForcePasswordChangeError
{
    public record NotFound : ForcePasswordChangeError;
}

public record ForcePasswordChangeCommand(int CustomerId, ForcePasswordChangeRequest Request) : IRequest<object>;

public class ForcePasswordChangeHandler(AppDbContext db) : IRequestHandler<ForcePasswordChangeCommand, object>
{
    private readonly PasswordHasher<Customer> _passwordHasher = new();

    public async Task<object> HandleAsync(ForcePasswordChangeCommand command, CancellationToken cancellationToken)
    {
        var customerId = command.CustomerId;
        var request = command.Request;

        var customer = await db.Customers
            .SingleOrDefaultAsync(c => c.Id == customerId, cancellationToken);

        if (customer is null)
            return new ForcePasswordChangeError.NotFound();

        customer.PasswordHash = _passwordHasher.HashPassword(customer, request.NewPassword);
        customer.MustChangePassword = false;

        await db.SaveChangesAsync(cancellationToken);

        return new ForcePasswordChangeSuccess();
    }
}