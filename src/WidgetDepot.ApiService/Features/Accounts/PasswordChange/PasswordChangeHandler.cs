using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using WidgetDepot.ApiService.Data;

namespace WidgetDepot.ApiService.Features.Accounts.PasswordChange;

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);

public record ChangePasswordSuccess;

public abstract record PasswordChangeError
{
    public record NotFound : PasswordChangeError;
    public record IncorrectPassword : PasswordChangeError;
}

public class PasswordChangeHandler(AppDbContext db)
{
    private readonly PasswordHasher<Customer> _passwordHasher = new();

    public async Task<object> ChangeAsync(int customerId, ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var customer = await db.Customers
            .SingleOrDefaultAsync(c => c.Id == customerId, cancellationToken);

        if (customer is null)
            return new PasswordChangeError.NotFound();

        var verifyResult = _passwordHasher.VerifyHashedPassword(customer, customer.PasswordHash, request.CurrentPassword);

        if (verifyResult == PasswordVerificationResult.Failed)
            return new PasswordChangeError.IncorrectPassword();

        customer.PasswordHash = _passwordHasher.HashPassword(customer, request.NewPassword);

        await db.SaveChangesAsync(cancellationToken);

        return new ChangePasswordSuccess();
    }
}