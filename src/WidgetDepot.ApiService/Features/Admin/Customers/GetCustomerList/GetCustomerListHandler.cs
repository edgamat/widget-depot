using Microsoft.EntityFrameworkCore;

using WidgetDepot.ApiService.Data;
using WidgetDepot.ApiService.Shared;

namespace WidgetDepot.ApiService.Features.Admin.Customers.GetCustomerList;

public record CustomerListItem(int Id, string FirstName, string LastName, string Email, bool IsAdmin);

public record GetCustomerListResponse(IReadOnlyList<CustomerListItem> Customers, int TotalCount, int Page, int PageSize);

public record GetCustomerListQuery(int Page, int PageSize) : IRequest<GetCustomerListResponse>;

public class GetCustomerListHandler(AppDbContext db) : IRequestHandler<GetCustomerListQuery, GetCustomerListResponse>
{
    public async Task<GetCustomerListResponse> HandleAsync(GetCustomerListQuery query, CancellationToken cancellationToken)
    {
        var page = query.Page;
        var pageSize = query.PageSize;

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