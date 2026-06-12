using System.Net;
using System.Text.Json;

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

    public async Task<UpdateEmailResult> UpdateEmailAsync(int id, string email, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PutAsJsonAsync($"/admin/customers/{id}/email", new { Email = email }, cancellationToken);

        if (response.IsSuccessStatusCode)
            return new UpdateEmailResult.Success();

        if (response.StatusCode == HttpStatusCode.Conflict)
            return new UpdateEmailResult.EmailAlreadyInUse();

        return new UpdateEmailResult.Failure();
    }

    public async Task<ResetPasswordResult> ResetPasswordAsync(int id, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsync($"/admin/customers/{id}/reset-password", null, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(body);
            var tempPassword = doc.RootElement.GetProperty("temporaryPassword").GetString() ?? string.Empty;
            return new ResetPasswordResult.Success(tempPassword);
        }

        return new ResetPasswordResult.Failure();
    }

    public async Task<PromoteResult> PromoteAsync(int id, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PutAsync($"/admin/customers/{id}/promote", null, cancellationToken);

        if (response.IsSuccessStatusCode)
            return new PromoteResult.Success();

        return new PromoteResult.Failure();
    }

    public async Task<DemoteResult> DemoteAsync(int id, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PutAsync($"/admin/customers/{id}/demote", null, cancellationToken);

        if (response.IsSuccessStatusCode)
            return new DemoteResult.Success();

        return new DemoteResult.Failure();
    }
}