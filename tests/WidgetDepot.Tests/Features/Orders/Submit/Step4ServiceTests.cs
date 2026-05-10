using System.Net;
using System.Text;

using Shouldly;

using WidgetDepot.Web.Features.Orders.Submit;

namespace WidgetDepot.Tests.Features.Orders.Submit;

public class Step4ServiceTests
{
    private static Step4Service CreateService(HttpStatusCode statusCode, string? responseContent = null)
    {
        var handler = new FakeHttpMessageHandler(statusCode, responseContent);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://test") };
        return new Step4Service(httpClient);
    }

    [Fact]
    public async Task GetDraftOrderAsync_OkResponse_ReturnsSuccessWithOrder()
    {
        var json = """{"id":7,"status":"Draft","items":[{"widgetId":1,"sku":"W-001","name":"Sprocket","weight":2.0,"quantity":3}],"shippingAddress":{"recipientName":"Alice","streetLine1":"123 Main","streetLine2":null,"city":"Springfield","state":"IL","zipCode":"62701"},"billingAddress":null,"shippingEstimate":14.99}""";
        var service = CreateService(HttpStatusCode.OK, json);

        var result = await service.GetDraftOrderAsync(7, TestContext.Current.CancellationToken);

        var success = result.ShouldBeOfType<GetDraftOrderResult.Success>();
        success.Order.Id.ShouldBe(7);
        success.Order.Items.Count.ShouldBe(1);
        success.Order.ShippingEstimate.ShouldBe(14.99m);
    }

    [Fact]
    public async Task GetDraftOrderAsync_NotFoundResponse_ReturnsNotFound()
    {
        var service = CreateService(HttpStatusCode.NotFound);

        var result = await service.GetDraftOrderAsync(999, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<GetDraftOrderResult.NotFound>();
    }

    [Fact]
    public async Task GetDraftOrderAsync_ForbiddenResponse_ReturnsForbidden()
    {
        var service = CreateService(HttpStatusCode.Forbidden);

        var result = await service.GetDraftOrderAsync(7, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<GetDraftOrderResult.Forbidden>();
    }

    [Fact]
    public async Task GetDraftOrderAsync_ServerErrorResponse_ReturnsFailure()
    {
        var service = CreateService(HttpStatusCode.InternalServerError);

        var result = await service.GetDraftOrderAsync(7, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<GetDraftOrderResult.Failure>();
    }

    [Fact]
    public async Task SubmitOrderAsync_OkResponse_ReturnsSuccessWithOrderId()
    {
        var json = """{"orderId":7}""";
        var service = CreateService(HttpStatusCode.OK, json);

        var result = await service.SubmitOrderAsync(7, TestContext.Current.CancellationToken);

        var success = result.ShouldBeOfType<SubmitOrderResult.Success>();
        success.OrderId.ShouldBe(7);
    }

    [Fact]
    public async Task SubmitOrderAsync_NotFoundResponse_ReturnsNotFound()
    {
        var service = CreateService(HttpStatusCode.NotFound);

        var result = await service.SubmitOrderAsync(999, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<SubmitOrderResult.NotFound>();
    }

    [Fact]
    public async Task SubmitOrderAsync_ForbiddenResponse_ReturnsForbidden()
    {
        var service = CreateService(HttpStatusCode.Forbidden);

        var result = await service.SubmitOrderAsync(7, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<SubmitOrderResult.Forbidden>();
    }

    [Fact]
    public async Task SubmitOrderAsync_ConflictResponse_ReturnsInvalidState()
    {
        var service = CreateService(HttpStatusCode.Conflict);

        var result = await service.SubmitOrderAsync(7, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<SubmitOrderResult.InvalidState>();
    }

    [Fact]
    public async Task SubmitOrderAsync_UnprocessableEntityResponse_ReturnsIncompleteOrder()
    {
        var service = CreateService(HttpStatusCode.UnprocessableEntity);

        var result = await service.SubmitOrderAsync(7, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<SubmitOrderResult.IncompleteOrder>();
    }

    [Fact]
    public async Task SubmitOrderAsync_ServerErrorResponse_ReturnsFailure()
    {
        var service = CreateService(HttpStatusCode.InternalServerError);

        var result = await service.SubmitOrderAsync(7, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<SubmitOrderResult.Failure>();
    }

    private class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;
        private readonly string _responseContent;

        public FakeHttpMessageHandler(HttpStatusCode statusCode, string? responseContent = null)
        {
            _statusCode = statusCode;
            _responseContent = responseContent ?? "{}";
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_responseContent, Encoding.UTF8, "application/json")
            };
            return Task.FromResult(response);
        }
    }
}