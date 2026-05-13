using System.Text;

using Shouldly;

using WidgetDepot.Web.Features.Orders.History;

namespace WidgetDepot.Tests.Features.Orders.History;

public class HistoryServiceTests
{
    private static HistoryService CreateService(HttpStatusCode statusCode, string responseBody)
    {
        var handler = new FakeHttpMessageHandler(statusCode, responseBody);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://test") };
        return new HistoryService(httpClient);
    }

    [Fact]
    public async Task GetRecentOrdersAsync_SuccessResponse_ReturnsOrders()
    {
        var body = """[{"id":1,"submittedAt":"2026-05-01T00:00:00","widgetCount":3,"shippingEstimate":12.50}]""";
        var service = CreateService(HttpStatusCode.OK, body);

        var result = await service.GetRecentOrdersAsync(TestContext.Current.CancellationToken);

        var success = result.ShouldBeOfType<GetRecentOrdersResult.Success>();
        success.Orders.Count.ShouldBe(1);
        success.Orders[0].Id.ShouldBe(1);
        success.Orders[0].WidgetCount.ShouldBe(3);
        success.Orders[0].ShippingEstimate.ShouldBe(12.50m);
    }

    [Fact]
    public async Task GetRecentOrdersAsync_EmptyResponse_ReturnsEmptyList()
    {
        var service = CreateService(HttpStatusCode.OK, "[]");

        var result = await service.GetRecentOrdersAsync(TestContext.Current.CancellationToken);

        var success = result.ShouldBeOfType<GetRecentOrdersResult.Success>();
        success.Orders.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetRecentOrdersAsync_ServerError_ReturnsFailure()
    {
        var service = CreateService(HttpStatusCode.InternalServerError, "{}");

        var result = await service.GetRecentOrdersAsync(TestContext.Current.CancellationToken);

        result.ShouldBeOfType<GetRecentOrdersResult.Failure>();
    }

    [Fact]
    public async Task GetRecentOrdersAsync_Unauthorized_ReturnsFailure()
    {
        var service = CreateService(HttpStatusCode.Unauthorized, "{}");

        var result = await service.GetRecentOrdersAsync(TestContext.Current.CancellationToken);

        result.ShouldBeOfType<GetRecentOrdersResult.Failure>();
    }

    [Fact]
    public async Task GetRecentOrdersAsync_NullShippingEstimate_ReturnsNullEstimate()
    {
        var body = """[{"id":2,"submittedAt":"2026-05-01T00:00:00","widgetCount":1,"shippingEstimate":null}]""";
        var service = CreateService(HttpStatusCode.OK, body);

        var result = await service.GetRecentOrdersAsync(TestContext.Current.CancellationToken);

        var success = result.ShouldBeOfType<GetRecentOrdersResult.Success>();
        success.Orders[0].ShippingEstimate.ShouldBeNull();
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