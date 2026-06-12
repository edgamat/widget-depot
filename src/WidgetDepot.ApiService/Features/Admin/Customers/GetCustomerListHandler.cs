using Microsoft.EntityFrameworkCore;

using WidgetDepot.ApiService.Data;

namespace WidgetDepot.ApiService.Features.Admin.Customers;

public record CustomerListItem(int Id, string FirstName, string LastName, string Email, bool IsAdmin);

public record GetCustomerListResponse(IReadOnlyList<CustomerListItem> Customers, int TotalCount, int Page, int PageSize);

public class GetCustomerListHandler(AppDbContext db)
{
    public async Task<GetCustomerListResponse> GetAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        var totalCount = await db.Customers.CountAsync(cancellationToken);

        var customers = await db.Customers
            .OrderBy(c => c.LastName)
            .ThenBy(c => c.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CustomerListItem(c.Id, c.FirstName, c.LastName, c.Email, c.IsAdmin))
            .ToListAsync(cancellationToken);

        return new GetCustomerListResponse(customers, totalCount, page, pageSize);
    }
}