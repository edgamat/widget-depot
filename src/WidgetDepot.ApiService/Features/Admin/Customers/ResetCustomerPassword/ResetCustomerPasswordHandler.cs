using System.Security.Cryptography;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using WidgetDepot.ApiService.Data;
using WidgetDepot.ApiService.Shared;

namespace WidgetDepot.ApiService.Features.Admin.Customers.ResetCustomerPassword;

public record ResetCustomerPasswordSuccess(string TemporaryPassword);

public abstract record ResetCustomerPasswordError
{
    public record NotFound : ResetCustomerPasswordError;
}

public record ResetCustomerPasswordCommand(int CustomerId) : IRequest<object>;

public class ResetCustomerPasswordHandler(AppDbContext db) : IRequestHandler<ResetCustomerPasswordCommand, object>
{
    private readonly PasswordHasher<Customer> _passwordHasher = new();

    public async Task<object> HandleAsync(ResetCustomerPasswordCommand command, CancellationToken cancellationToken)
    {
        var customerId = command.CustomerId;

        var customer = await db.Customers
            .SingleOrDefaultAsync(c => c.Id == customerId, cancellationToken);

        if (customer is null)
            return new ResetCustomerPasswordError.NotFound();

        var temporaryPassword = GenerateTemporaryPassword();
        customer.PasswordHash = _passwordHasher.HashPassword(customer, temporaryPassword);
        customer.MustChangePassword = true;

        await db.SaveChangesAsync(cancellationToken);

        return new ResetCustomerPasswordSuccess(temporaryPassword);
    }

    private static string GenerateTemporaryPassword()
    {
        const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var bytes = new byte[12];
        RandomNumberGenerator.Fill(bytes);
        var suffix = new string(bytes.Select(b => chars[b % chars.Length]).ToArray());
        return "Tmp-" + suffix;
    }
}