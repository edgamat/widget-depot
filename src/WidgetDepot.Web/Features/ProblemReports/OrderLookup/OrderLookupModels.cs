using System.ComponentModel.DataAnnotations;

namespace WidgetDepot.Web.Features.ProblemReports.OrderLookup;

public class OrderLookupFormModel
{
    [Required(ErrorMessage = "Order number is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Order number must be a positive number.")]
    public int? OrderNumber { get; set; }
}

public record ProblemReportOrderItem(string WidgetName, int Quantity);

public abstract record LookupOrderResult
{
    public record Success(int OrderId, IReadOnlyList<ProblemReportOrderItem> Items) : LookupOrderResult;
    public record NotFound : LookupOrderResult;
    public record NotSubmitted : LookupOrderResult;
    public record Failure : LookupOrderResult;
}

internal record GetOrderForProblemReportItemResponse(string WidgetName, int Quantity);

internal record GetOrderForProblemReportResponse(int OrderId, IReadOnlyList<GetOrderForProblemReportItemResponse> Items);