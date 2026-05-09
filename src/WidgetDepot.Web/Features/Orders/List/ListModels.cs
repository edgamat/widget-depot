namespace WidgetDepot.Web.Features.Orders.List;

public record DraftOrderListItem(int Id, int WidgetCount, DateTime CreatedAt, DateTime ExpiryDate);

public abstract record GetDraftsResult
{
    public record Success(IReadOnlyList<DraftOrderListItem> Drafts) : GetDraftsResult;
    public record Failure : GetDraftsResult;
}

public abstract record DeleteDraftResult
{
    public record Success : DeleteDraftResult;
    public record NotFound : DeleteDraftResult;
    public record Forbidden : DeleteDraftResult;
    public record Failure : DeleteDraftResult;
}

internal record GetDraftsResponse(int Id, int WidgetCount, DateTime CreatedAt);