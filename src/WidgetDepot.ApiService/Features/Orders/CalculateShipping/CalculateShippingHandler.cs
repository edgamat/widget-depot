using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using WidgetDepot.ApiService.Data;

namespace WidgetDepot.ApiService.Features.Orders.CalculateShipping;

public record CalculateShippingResponse(decimal EstimatedCost, string Currency);

public abstract record CalculateShippingError
{
    public record OrderNotFound : CalculateShippingError;
    public record Forbidden : CalculateShippingError;
    public record NoShippingAddress : CalculateShippingError;
    public record ShippingApiFailure(string Reason) : CalculateShippingError;
}

public class CalculateShippingHandler
{
    private readonly AppDbContext _db;
    private readonly IShippingApiClient _shippingApiClient;
    private readonly string _originPostalCode;
    private readonly string _originCountry;

    public CalculateShippingHandler(AppDbContext db, IShippingApiClient shippingApiClient, IConfiguration configuration)
    {
        _db = db;
        _shippingApiClient = shippingApiClient;
        _originPostalCode = configuration["Shipping:OriginPostalCode"] ?? "10001";
        _originCountry = configuration["Shipping:OriginCountry"] ?? "US";
    }

    public async Task<object> HandleAsync(int orderId, int customerId, CancellationToken cancellationToken)
    {
        var order = await _db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (order is null)
            return new CalculateShippingError.OrderNotFound();

        if (order.CustomerId != customerId)
            return new CalculateShippingError.Forbidden();

        if (order.ShippingAddress is null)
            return new CalculateShippingError.NoShippingAddress();

        var totalWeight = await CalculateTotalWeightAsync(order.Items, cancellationToken);

        var request = new ShippingEstimateRequest(
            _originPostalCode,
            _originCountry,
            order.ShippingAddress.ZipCode,
            "US",
            totalWeight);

        var result = await _shippingApiClient.GetEstimateAsync(request, cancellationToken);

        if (result is ShippingEstimateResult.Failure failure)
            return new CalculateShippingError.ShippingApiFailure(failure.Reason);

        var success = (ShippingEstimateResult.Success)result;
        order.ShippingEstimate = success.EstimatedCost;
        await _db.SaveChangesAsync(cancellationToken);

        return new CalculateShippingResponse(success.EstimatedCost, success.Currency);
    }

    private async Task<decimal> CalculateTotalWeightAsync(ICollection<OrderItem> items, CancellationToken cancellationToken)
    {
        var widgetIds = items.Select(i => i.WidgetId).ToList();

        var widgetWeights = await _db.Widgets
            .Where(w => widgetIds.Contains(w.Id))
            .ToDictionaryAsync(w => w.Id, w => w.Weight, cancellationToken);

        return items.Sum(i => i.Quantity * widgetWeights.GetValueOrDefault(i.WidgetId, 0m));
    }
}