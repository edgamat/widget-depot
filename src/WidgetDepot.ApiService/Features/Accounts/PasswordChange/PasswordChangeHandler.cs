using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using WidgetDepot.ApiService.Data;
using WidgetDepot.ApiService.Shared;

namespace WidgetDepot.ApiService.Features.Accounts.PasswordChange;

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);

public record ChangePasswordSuccess;

public abstract record PasswordChangeError
{
    public record NotFound : PasswordChangeError;
    public record IncorrectPassword : PasswordChangeError;
}

public record ChangePasswordCommand(int CustomerId, ChangePasswordRequest Request) : IRequest<object>;

public class PasswordChangeHandler(AppDbContext db) : IRequestHandler<ChangePasswordCommand, object>
{
    private readonly PasswordHasher<Customer> _passwordHasher = new();

    public async Task<object> HandleAsync(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        var customerId = command.CustomerId;
        var request = command.Request;

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