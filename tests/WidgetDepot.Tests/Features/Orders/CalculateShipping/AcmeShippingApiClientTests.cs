using System.Text;

using Shouldly;

using WidgetDepot.ApiService.Features.Orders.CalculateShipping;

namespace WidgetDepot.Tests.Features.Orders.CalculateShipping;

public class AcmeShippingApiClientTests
{
    private static HttpClient CreateClient(HttpMessageHandler handler)
    {
        return new HttpClient(handler) { BaseAddress = new Uri("https://api.acmeshipping.com/v1/") };
    }

    private static ShippingEstimateRequest ValidRequest() =>
        new("12345", "US", "67890", "US", 5.0m);

    [Fact]
    public async Task GetEstimateAsync_SuccessResponse_ReturnsSuccess()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, """{"estimatedCost":12.50,"currency":"USD"}""");
        var apiClient = new AcmeShippingApiClient(CreateClient(handler));

        var result = await apiClient.GetEstimateAsync(ValidRequest(), CancellationToken.None);

        var success = result.ShouldBeOfType<ShippingEstimateResult.Success>();
        success.EstimatedCost.ShouldBe(12.50m);
        success.Currency.ShouldBe("USD");
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public async Task GetEstimateAsync_ErrorStatusCode_ReturnsFailure(HttpStatusCode statusCode)
    {
        var handler = new FakeHttpMessageHandler(statusCode, """{"error":"error","message":"msg"}""");
        var apiClient = new AcmeShippingApiClient(CreateClient(handler));

        var result = await apiClient.GetEstimateAsync(ValidRequest(), CancellationToken.None);

        result.ShouldBeOfType<ShippingEstimateResult.Failure>();
    }

    [Fact]
    public async Task GetEstimateAsync_NetworkFailure_ReturnsFailure()
    {
        var apiClient = new AcmeShippingApiClient(CreateClient(new NetworkErrorHttpMessageHandler()));

        var result = await apiClient.GetEstimateAsync(ValidRequest(), CancellationToken.None);

        result.ShouldBeOfType<ShippingEstimateResult.Failure>();
    }

    private class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;
        private readonly string _content;

        public FakeHttpMessageHandler(HttpStatusCode statusCode, string content)
        {
            _statusCode = statusCode;
            _content = content;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_content, Encoding.UTF8, "application/json")
            });
        }
    }

    private class NetworkErrorHttpMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            throw new HttpRequestException("Network error");
        }
    }
}