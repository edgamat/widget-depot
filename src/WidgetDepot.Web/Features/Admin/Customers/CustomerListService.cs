using Microsoft.Extensions.Options;

namespace WidgetDepot.Web.Features.Admin.Customers;

public class CustomerListService(HttpClient httpClient, IOptions<PaginationOptions> paginationOptions)
{
    public int PageSize { get; } = paginationOptions.Value.PageSize;

    public async Task<GetCustomersResponse?> GetCustomersAsync(int page, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync(
            $"/admin/customers?page={page}&pageSize={PageSize}",
            cancellationToken);

        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<GetCustomersResponse>(cancellationToken);
    }

    public async Task<CustomerProfileDto?> GetCustomerProfileAsync(int id, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync($"/admin/customers/{id}", cancellationToken);

        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<CustomerProfileDto>(cancellationToken);
    }
}