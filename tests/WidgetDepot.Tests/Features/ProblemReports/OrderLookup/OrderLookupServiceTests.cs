using System.Text;

using Shouldly;

using WidgetDepot.Web.Features.ProblemReports.OrderLookup;

namespace WidgetDepot.Tests.Features.ProblemReports.OrderLookup;

public class OrderLookupServiceTests
{
    private static OrderLookupService CreateService(HttpStatusCode statusCode, string responseBody)
    {
        var handler = new FakeHttpMessageHandler(statusCode, responseBody);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://test") };
        return new OrderLookupService(httpClient);
    }

    [Fact]
    public async Task LookupOrderAsync_OrderFound_ReturnsSuccess()
    {
        var body = """
            {
                "orderId": 42,
                "items": [
                    { "widgetName": "Sprocket", "quantity": 2 }
                ]
            }
            """;
        var service = CreateService(HttpStatusCode.OK, body);

        var result = await service.LookupOrderAsync(42, TestContext.Current.CancellationToken);

        var success = result.ShouldBeOfType<LookupOrderResult.Success>();
        success.OrderId.ShouldBe(42);
        success.Items.Count.ShouldBe(1);
        success.Items[0].WidgetName.ShouldBe("Sprocket");
        success.Items[0].Quantity.ShouldBe(2);
    }

    [Fact]
    public async Task LookupOrderAsync_OrderNotFound_ReturnsNotFound()
    {
        var service = CreateService(HttpStatusCode.NotFound, "{}");

        var result = await service.LookupOrderAsync(999, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<LookupOrderResult.NotFound>();
    }

    [Fact]
    public async Task LookupOrderAsync_OrderNotSubmitted_ReturnsNotSubmitted()
    {
        var service = CreateService(HttpStatusCode.UnprocessableEntity, "{}");

        var result = await service.LookupOrderAsync(1, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<LookupOrderResult.NotSubmitted>();
    }

    [Fact]
    public async Task LookupOrderAsync_ServerError_ReturnsFailure()
    {
        var service = CreateService(HttpStatusCode.InternalServerError, "{}");

        var result = await service.LookupOrderAsync(1, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<LookupOrderResult.Failure>();
    }

    [Fact]
    public async Task LookupOrderAsync_Unauthorized_ReturnsFailure()
    {
        var service = CreateService(HttpStatusCode.Unauthorized, "{}");

        var result = await service.LookupOrderAsync(1, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<LookupOrderResult.Failure>();
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