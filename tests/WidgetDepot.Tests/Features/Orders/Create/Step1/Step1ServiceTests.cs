using System.Text;

using Shouldly;

using WidgetDepot.Web.Features.Orders.Create.Step1;

namespace WidgetDepot.Tests.Features.Orders.Create.Step1;

public class Step1ServiceTests
{
    private static Step1Service CreateService(HttpStatusCode statusCode, string responseBody)
    {
        var handler = new FakeHttpMessageHandler(statusCode, responseBody);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://test") };
        return new Step1Service(httpClient);
    }

    [Fact]
    public async Task SearchWidgetsAsync_SuccessResponse_ReturnsWidgets()
    {
        var body = """[{"id":1,"sku":"SPR-001","name":"Sprocket","description":"A sprocket","manufacturer":"Acme","weight":1.5,"price":9.99,"dateAvailable":"2026-01-01"}]""";
        var service = CreateService(HttpStatusCode.OK, body);

        var results = await service.SearchWidgetsAsync("sprocket", TestContext.Current.CancellationToken);

        results.Count.ShouldBe(1);
        results[0].Name.ShouldBe("Sprocket");
    }

    [Fact]
    public async Task SearchWidgetsAsync_EmptyResponse_ReturnsEmpty()
    {
        var service = CreateService(HttpStatusCode.OK, "[]");

        var results = await service.SearchWidgetsAsync("nothing", TestContext.Current.CancellationToken);

        results.ShouldBeEmpty();
    }

    [Fact]
    public async Task CreateDraftAsync_SuccessResponse_ReturnsSuccess()
    {
        var service = CreateService(HttpStatusCode.OK, """{"orderId":7}""");
        var items = new List<OrderItemModel>
        {
            new() { WidgetId = 1, Sku = "SPR-001", Name = "Sprocket", Weight = 1.5m, Quantity = 2 }
        };

        var result = await service.CreateDraftAsync(items, TestContext.Current.CancellationToken);

        var success = result.ShouldBeOfType<CreateDraftResult.Success>();
        success.OrderId.ShouldBe(7);
    }

    [Fact]
    public async Task CreateDraftAsync_ServerError_ReturnsFailure()
    {
        var service = CreateService(HttpStatusCode.InternalServerError, "{}");
        var items = new List<OrderItemModel>
        {
            new() { WidgetId = 1, Sku = "SPR-001", Name = "Sprocket", Weight = 1.5m, Quantity = 1 }
        };

        var result = await service.CreateDraftAsync(items, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<CreateDraftResult.Failure>();
    }

    [Fact]
    public async Task CreateDraftAsync_Unauthorized_ReturnsFailure()
    {
        var service = CreateService(HttpStatusCode.Unauthorized, "{}");
        var items = new List<OrderItemModel>
        {
            new() { WidgetId = 1, Sku = "SPR-001", Name = "Sprocket", Weight = 1.5m, Quantity = 1 }
        };

        var result = await service.CreateDraftAsync(items, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<CreateDraftResult.Failure>();
    }

    [Fact]
    public async Task GetDraftOrderAsync_SuccessResponse_ReturnsSuccessWithItems()
    {
        var body = """{"id":5,"status":"Draft","items":[{"widgetId":1,"sku":"SPR-001","name":"Sprocket","weight":1.5,"quantity":2}]}""";
        var service = CreateService(HttpStatusCode.OK, body);

        var result = await service.GetDraftOrderAsync(5, TestContext.Current.CancellationToken);

        var success = result.ShouldBeOfType<GetDraftStep1Result.Success>();
        success.Order.Items.Count.ShouldBe(1);
        success.Order.Items[0].Sku.ShouldBe("SPR-001");
        success.Order.Items[0].Quantity.ShouldBe(2);
    }

    [Fact]
    public async Task GetDraftOrderAsync_NotFound_ReturnsNotFound()
    {
        var service = CreateService(HttpStatusCode.NotFound, "{}");

        var result = await service.GetDraftOrderAsync(5, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<GetDraftStep1Result.NotFound>();
    }

    [Fact]
    public async Task GetDraftOrderAsync_Forbidden_ReturnsForbidden()
    {
        var service = CreateService(HttpStatusCode.Forbidden, "{}");

        var result = await service.GetDraftOrderAsync(5, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<GetDraftStep1Result.Forbidden>();
    }

    [Fact]
    public async Task GetDraftOrderAsync_ServerError_ReturnsFailure()
    {
        var service = CreateService(HttpStatusCode.InternalServerError, "{}");

        var result = await service.GetDraftOrderAsync(5, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<GetDraftStep1Result.Failure>();
    }

    [Fact]
    public async Task UpdateDraftAsync_NoContentResponse_ReturnsSuccess()
    {
        var service = CreateService(HttpStatusCode.NoContent, "");
        var items = new List<OrderItemModel>
        {
            new() { WidgetId = 1, Sku = "SPR-001", Name = "Sprocket", Weight = 1.5m, Quantity = 3 }
        };

        var result = await service.UpdateDraftAsync(5, items, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<UpdateDraftResult.Success>();
    }

    [Fact]
    public async Task UpdateDraftAsync_ServerError_ReturnsFailure()
    {
        var service = CreateService(HttpStatusCode.InternalServerError, "{}");
        var items = new List<OrderItemModel>
        {
            new() { WidgetId = 1, Sku = "SPR-001", Name = "Sprocket", Weight = 1.5m, Quantity = 1 }
        };

        var result = await service.UpdateDraftAsync(5, items, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<UpdateDraftResult.Failure>();
    }

    [Fact]
    public async Task UpdateDraftAsync_NotFound_ReturnsFailure()
    {
        var service = CreateService(HttpStatusCode.NotFound, "{}");
        var items = new List<OrderItemModel>
        {
            new() { WidgetId = 1, Sku = "SPR-001", Name = "Sprocket", Weight = 1.5m, Quantity = 1 }
        };

        var result = await service.UpdateDraftAsync(5, items, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<UpdateDraftResult.Failure>();
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