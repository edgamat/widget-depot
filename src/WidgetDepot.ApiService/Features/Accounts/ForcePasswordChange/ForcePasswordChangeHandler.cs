using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using WidgetDepot.ApiService.Data;

namespace WidgetDepot.ApiService.Features.Accounts.ForcePasswordChange;

public record ForcePasswordChangeRequest(string NewPassword);

public record ForcePasswordChangeSuccess;

public abstract record ForcePasswordChangeError
{
    public record NotFound : ForcePasswordChangeError;
}

public class ForcePasswordChangeHandler(AppDbContext db)
{
    private readonly PasswordHasher<Customer> _passwordHasher = new();

    public async Task<object> ChangeAsync(int customerId, ForcePasswordChangeRequest request, CancellationToken cancellationToken)
    {
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