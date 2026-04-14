using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using WidgetDepot.ApiService.Data;

namespace WidgetDepot.ApiService.Features.Accounts.Register;

public record RegisterRequest(string FirstName, string LastName, string Email, string Password);

public record RegisterResponse(int CustomerId);

public abstract record RegisterError
{
    public record EmailAlreadyRegistered : RegisterError;
}

public class RegisterHandler(AppDbContext db)
{
    private readonly PasswordHasher<Customer> _passwordHasher = new();

    public async Task<object> HandleAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var emailExists = await db.Customers
            .AnyAsync(c => c.Email == request.Email, cancellationToken);

        if (emailExists)
            return new RegisterError.EmailAlreadyRegistered();

        var customer = new Customer
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            CreatedAt = DateTime.UtcNow
        };

        customer.PasswordHash = _passwordHasher.HashPassword(customer, request.Password);

        db.Customers.Add(customer);
        await db.SaveChangesAsync(cancellationToken);

        return new RegisterResponse(customer.Id);
    }
}