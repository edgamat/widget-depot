namespace WidgetDepot.Web.Features.Orders.Create.Step1;

public record WidgetSearchResult(
    int Id,
    string Sku,
    string Name,
    string Description,
    string Manufacturer,
    decimal Weight,
    decimal Price,
    DateOnly DateAvailable);

public class OrderItemModel
{
    public int WidgetId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public int Quantity { get; set; }
}

public record CreateDraftRequest(IReadOnlyList<CreateDraftItemRequest> Items);

public record CreateDraftItemRequest(int WidgetId, int Quantity);

public record CreateDraftResponse(int OrderId);

public abstract record CreateDraftResult
{
    public record Success(int OrderId) : CreateDraftResult;
    public record Failure : CreateDraftResult;
}

public record GetDraftStep1ItemResponse(int WidgetId, string Sku, string Name, decimal Weight, int Quantity);

public record GetDraftStep1Response(int Id, string Status, IReadOnlyList<GetDraftStep1ItemResponse> Items);

public abstract record GetDraftStep1Result
{
    public record Success(GetDraftStep1Response Order) : GetDraftStep1Result;
    public record NotFound : GetDraftStep1Result;
    public record Forbidden : GetDraftStep1Result;
    public record Failure : GetDraftStep1Result;
}