using System.Net;
using System.Text;

using Shouldly;

using WidgetDepot.Web.Features.Orders.Create.Step3;

namespace WidgetDepot.Tests.Features.Orders.Create.Step3;

public class Step3ServiceTests
{
    private static Step3Service CreateService(HttpStatusCode statusCode, string? responseContent = null)
    {
        var handler = new FakeHttpMessageHandler(statusCode, responseContent: responseContent);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://test") };
        return new Step3Service(httpClient);
    }

    [Fact]
    public async Task GetDraftOrderAsync_OkResponse_ReturnsSuccessWithOrder()
    {
        var json = """{"id":5,"status":"Draft","items":[{"widgetId":1,"quantity":2}],"shippingAddress":{"recipientName":"Alice","streetLine1":"123 Main","streetLine2":null,"city":"Springfield","state":"IL","zipCode":"62701"},"billingAddress":null}""";
        var service = CreateService(HttpStatusCode.OK, json);

        var result = await service.GetDraftOrderAsync(5, TestContext.Current.CancellationToken);

        var success = result.ShouldBeOfType<GetDraftOrderResult.Success>();
        success.Order.Id.ShouldBe(5);
        success.Order.Items.Count.ShouldBe(1);
        success.Order.ShippingAddress.ShouldNotBeNull();
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

        var result = await service.GetDraftOrderAsync(5, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<GetDraftOrderResult.Forbidden>();
    }

    [Fact]
    public async Task GetDraftOrderAsync_ServerErrorResponse_ReturnsFailure()
    {
        var service = CreateService(HttpStatusCode.InternalServerError);

        var result = await service.GetDraftOrderAsync(5, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<GetDraftOrderResult.Failure>();
    }

    [Fact]
    public async Task CalculateShippingAsync_OkResponse_ReturnsSuccessWithCostAndCurrency()
    {
        var json = """{"estimatedCost":12.50,"currency":"USD"}""";
        var service = CreateService(HttpStatusCode.OK, json);

        var result = await service.CalculateShippingAsync(5, TestContext.Current.CancellationToken);

        var success = result.ShouldBeOfType<CalculateShippingResult.Success>();
        success.EstimatedCost.ShouldBe(12.50m);
        success.Currency.ShouldBe("USD");
    }

    [Fact]
    public async Task CalculateShippingAsync_NotFoundResponse_ReturnsNotFound()
    {
        var service = CreateService(HttpStatusCode.NotFound);

        var result = await service.CalculateShippingAsync(999, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<CalculateShippingResult.NotFound>();
    }

    [Fact]
    public async Task CalculateShippingAsync_ForbiddenResponse_ReturnsForbidden()
    {
        var service = CreateService(HttpStatusCode.Forbidden);

        var result = await service.CalculateShippingAsync(5, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<CalculateShippingResult.Forbidden>();
    }

    [Fact]
    public async Task CalculateShippingAsync_UnprocessableEntityResponse_ReturnsNoShippingAddress()
    {
        var service = CreateService(HttpStatusCode.UnprocessableEntity);

        var result = await service.CalculateShippingAsync(5, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<CalculateShippingResult.NoShippingAddress>();
    }

    [Fact]
    public async Task CalculateShippingAsync_BadGatewayResponse_ReturnsApiFailure()
    {
        var service = CreateService(HttpStatusCode.BadGateway);

        var result = await service.CalculateShippingAsync(5, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<CalculateShippingResult.ApiFailure>();
    }

    [Fact]
    public async Task CalculateShippingAsync_ServerErrorResponse_ReturnsFailure()
    {
        var service = CreateService(HttpStatusCode.InternalServerError);

        var result = await service.CalculateShippingAsync(5, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<CalculateShippingResult.Failure>();
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