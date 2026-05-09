using System.Text;

using Shouldly;

using WidgetDepot.Web.Features.Orders.List;

namespace WidgetDepot.Tests.Features.Orders.List;

public class ListServiceTests
{
    private static ListService CreateService(HttpStatusCode statusCode, string responseBody)
    {
        var handler = new FakeHttpMessageHandler(statusCode, responseBody);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://test") };
        return new ListService(httpClient);
    }

    [Fact]
    public async Task GetDraftsAsync_SuccessResponse_ReturnsDrafts()
    {
        var body = """[{"id":1,"widgetCount":3,"createdAt":"2026-04-01T00:00:00"}]""";
        var service = CreateService(HttpStatusCode.OK, body);

        var result = await service.GetDraftsAsync(TestContext.Current.CancellationToken);

        var success = result.ShouldBeOfType<GetDraftsResult.Success>();
        success.Drafts.Count.ShouldBe(1);
        success.Drafts[0].Id.ShouldBe(1);
        success.Drafts[0].WidgetCount.ShouldBe(3);
    }

    [Fact]
    public async Task GetDraftsAsync_SuccessResponse_SetsExpiryDateTo30DaysAfterCreated()
    {
        var body = """[{"id":1,"widgetCount":2,"createdAt":"2026-04-01T00:00:00"}]""";
        var service = CreateService(HttpStatusCode.OK, body);

        var result = await service.GetDraftsAsync(TestContext.Current.CancellationToken);

        var success = result.ShouldBeOfType<GetDraftsResult.Success>();
        success.Drafts[0].ExpiryDate.ShouldBe(new DateTime(2026, 5, 1));
    }

    [Fact]
    public async Task GetDraftsAsync_EmptyResponse_ReturnsEmptyList()
    {
        var service = CreateService(HttpStatusCode.OK, "[]");

        var result = await service.GetDraftsAsync(TestContext.Current.CancellationToken);

        var success = result.ShouldBeOfType<GetDraftsResult.Success>();
        success.Drafts.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetDraftsAsync_ServerError_ReturnsFailure()
    {
        var service = CreateService(HttpStatusCode.InternalServerError, "{}");

        var result = await service.GetDraftsAsync(TestContext.Current.CancellationToken);

        result.ShouldBeOfType<GetDraftsResult.Failure>();
    }

    [Fact]
    public async Task GetDraftsAsync_Unauthorized_ReturnsFailure()
    {
        var service = CreateService(HttpStatusCode.Unauthorized, "{}");

        var result = await service.GetDraftsAsync(TestContext.Current.CancellationToken);

        result.ShouldBeOfType<GetDraftsResult.Failure>();
    }

    [Fact]
    public async Task DeleteDraftAsync_SuccessResponse_ReturnsSuccess()
    {
        var service = CreateService(HttpStatusCode.NoContent, "");

        var result = await service.DeleteDraftAsync(1, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<DeleteDraftResult.Success>();
    }

    [Fact]
    public async Task DeleteDraftAsync_NotFound_ReturnsNotFound()
    {
        var service = CreateService(HttpStatusCode.NotFound, "{}");

        var result = await service.DeleteDraftAsync(1, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<DeleteDraftResult.NotFound>();
    }

    [Fact]
    public async Task DeleteDraftAsync_Forbidden_ReturnsForbidden()
    {
        var service = CreateService(HttpStatusCode.Forbidden, "{}");

        var result = await service.DeleteDraftAsync(1, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<DeleteDraftResult.Forbidden>();
    }

    [Fact]
    public async Task DeleteDraftAsync_ServerError_ReturnsFailure()
    {
        var service = CreateService(HttpStatusCode.InternalServerError, "{}");

        var result = await service.DeleteDraftAsync(1, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<DeleteDraftResult.Failure>();
    }

    private class FakeHttpMessageHandler(HttpStatusCode statusCode, string body) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };
            return Task.FromResult(response);
        }
    }
}