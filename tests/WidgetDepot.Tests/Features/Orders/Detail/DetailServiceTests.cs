using System.Net;
using System.Text;

using Shouldly;

using WidgetDepot.Web.Features.Orders.Detail;

namespace WidgetDepot.Tests.Features.Orders.Detail;

public class DetailServiceTests
{
    private static DetailService CreateService(HttpStatusCode statusCode, string responseBody)
    {
        var handler = new FakeHttpMessageHandler(statusCode, responseBody);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://test") };
        return new DetailService(httpClient);
    }

    [Fact]
    public async Task GetOrderDetailAsync_OrderFound_ReturnsSuccess()
    {
        var body = """
            {
                "id": 42,
                "status": "Submitted",
                "createdAt": "2026-04-01T00:00:00",
                "submittedAt": "2026-04-02T00:00:00",
                "items": [],
                "shippingAddress": null,
                "billingAddress": null,
                "shippingEstimate": 15.00
            }
            """;
        var service = CreateService(HttpStatusCode.OK, body);

        var result = await service.GetOrderDetailAsync(42, TestContext.Current.CancellationToken);

        var success = result.ShouldBeOfType<GetOrderDetailResult.Success>();
        success.Order.Id.ShouldBe(42);
        success.Order.Status.ShouldBe("Submitted");
        success.Order.SubmittedAt.ShouldNotBeNull();
        success.Order.ShippingEstimate.ShouldBe(15.00m);
    }

    [Fact]
    public async Task GetOrderDetailAsync_OrderNotFound_ReturnsNotFound()
    {
        var service = CreateService(HttpStatusCode.NotFound, "{}");

        var result = await service.GetOrderDetailAsync(999, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<GetOrderDetailResult.NotFound>();
    }

    [Fact]
    public async Task GetOrderDetailAsync_ServerError_ReturnsFailure()
    {
        var service = CreateService(HttpStatusCode.InternalServerError, "{}");

        var result = await service.GetOrderDetailAsync(1, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<GetOrderDetailResult.Failure>();
    }

    [Fact]
    public async Task GetOrderDetailAsync_Unauthorized_ReturnsFailure()
    {
        var service = CreateService(HttpStatusCode.Unauthorized, "{}");

        var result = await service.GetOrderDetailAsync(1, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<GetOrderDetailResult.Failure>();
    }

    [Fact]
    public async Task GetOrderDetailAsync_OrderWithItems_MapsItemsCorrectly()
    {
        var body = """
            {
                "id": 1,
                "status": "Draft",
                "createdAt": "2026-04-01T00:00:00",
                "submittedAt": null,
                "items": [
                    { "widgetId": 10, "sku": "SPR-001", "name": "Sprocket", "weight": 1.5, "unitCost": 9.99, "quantity": 3 }
                ],
                "shippingAddress": null,
                "billingAddress": null,
                "shippingEstimate": null
            }
            """;
        var service = CreateService(HttpStatusCode.OK, body);

        var result = await service.GetOrderDetailAsync(1, TestContext.Current.CancellationToken);

        var success = result.ShouldBeOfType<GetOrderDetailResult.Success>();
        success.Order.Items.Count.ShouldBe(1);
        success.Order.Items[0].Sku.ShouldBe("SPR-001");
        success.Order.Items[0].Quantity.ShouldBe(3);
        success.Order.Items[0].UnitCost.ShouldBe(9.99m);
    }

    [Fact]
    public async Task GetOrderDetailAsync_OrderWithAddresses_MapsAddressesCorrectly()
    {
        var body = """
            {
                "id": 1,
                "status": "Submitted",
                "createdAt": "2026-04-01T00:00:00",
                "submittedAt": "2026-04-02T00:00:00",
                "items": [],
                "shippingAddress": {
                    "recipientName": "Jane Doe",
                    "streetLine1": "123 Main St",
                    "streetLine2": null,
                    "city": "Springfield",
                    "state": "IL",
                    "zipCode": "62701"
                },
                "billingAddress": {
                    "recipientName": "Jane Doe",
                    "streetLine1": "456 Elm St",
                    "streetLine2": "Apt 2",
                    "city": "Shelbyville",
                    "state": "IL",
                    "zipCode": "62565"
                },
                "shippingEstimate": null
            }
            """;
        var service = CreateService(HttpStatusCode.OK, body);

        var result = await service.GetOrderDetailAsync(1, TestContext.Current.CancellationToken);

        var success = result.ShouldBeOfType<GetOrderDetailResult.Success>();
        success.Order.ShippingAddress.ShouldNotBeNull();
        success.Order.ShippingAddress.RecipientName.ShouldBe("Jane Doe");
        success.Order.ShippingAddress.City.ShouldBe("Springfield");
        success.Order.BillingAddress.ShouldNotBeNull();
        success.Order.BillingAddress.StreetLine2.ShouldBe("Apt 2");
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