using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using WidgetDepot.ApiService.Data;

namespace WidgetDepot.ApiService.Features.Accounts.Login;

public record LoginRequest(string Email, string Password);

public record LoginResponse(int CustomerId, string Email, string FirstName);

public abstract record LoginError
{
    public record InvalidCredentials : LoginError;
    public record Failure(string Message) : LoginError;
}

public class LoginHandler(AppDbContext db)
{
    private readonly PasswordHasher<Customer> _passwordHasher = new();

    public async Task<object> HandleAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var customer = await db.Customers
                .SingleOrDefaultAsync(c => c.Email == request.Email, cancellationToken);

            if (customer is null)
                return new LoginError.InvalidCredentials();

            var result = _passwordHasher.VerifyHashedPassword(customer, customer.PasswordHash, request.Password);

            if (result == PasswordVerificationResult.Failed)
                return new LoginError.InvalidCredentials();

            return new LoginResponse(customer.Id, customer.Email, customer.FirstName);
        }
        catch (Exception ex)
        {
            return new LoginError.Failure(ex.Message);
        }
    }
}