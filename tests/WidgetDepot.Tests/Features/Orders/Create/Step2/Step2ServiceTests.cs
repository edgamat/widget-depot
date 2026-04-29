using System.Net;
using System.Text;

using Shouldly;

using WidgetDepot.Web.Features.Orders.Create.Step2;

namespace WidgetDepot.Tests.Features.Orders.Create.Step2;

public class Step2ServiceTests
{
    private static Step2Service CreateService(HttpStatusCode statusCode)
    {
        var handler = new FakeHttpMessageHandler(statusCode);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://test") };
        return new Step2Service(httpClient);
    }

    private static Step2FormModel ValidForm() => new()
    {
        ShippingRecipientName = "Alice Smith",
        ShippingStreetLine1 = "123 Main St",
        ShippingStreetLine2 = null,
        ShippingCity = "Springfield",
        ShippingState = "IL",
        ShippingZipCode = "62701",
        BillingRecipientName = "Bob Jones",
        BillingStreetLine1 = "456 Oak Ave",
        BillingStreetLine2 = "Apt 2",
        BillingCity = "Shelbyville",
        BillingState = "IL",
        BillingZipCode = "62565-1234"
    };

    [Fact]
    public async Task SaveAddressesAsync_NoContentResponse_ReturnsSuccess()
    {
        var service = CreateService(HttpStatusCode.NoContent);

        var result = await service.SaveAddressesAsync(1, ValidForm(), TestContext.Current.CancellationToken);

        result.ShouldBeOfType<SaveAddressesResult.Success>();
    }

    [Fact]
    public async Task SaveAddressesAsync_NotFoundResponse_ReturnsNotFound()
    {
        var service = CreateService(HttpStatusCode.NotFound);

        var result = await service.SaveAddressesAsync(999, ValidForm(), TestContext.Current.CancellationToken);

        result.ShouldBeOfType<SaveAddressesResult.NotFound>();
    }

    [Fact]
    public async Task SaveAddressesAsync_ForbiddenResponse_ReturnsForbidden()
    {
        var service = CreateService(HttpStatusCode.Forbidden);

        var result = await service.SaveAddressesAsync(1, ValidForm(), TestContext.Current.CancellationToken);

        result.ShouldBeOfType<SaveAddressesResult.Forbidden>();
    }

    [Fact]
    public async Task SaveAddressesAsync_ServerErrorResponse_ReturnsFailure()
    {
        var service = CreateService(HttpStatusCode.InternalServerError);

        var result = await service.SaveAddressesAsync(1, ValidForm(), TestContext.Current.CancellationToken);

        result.ShouldBeOfType<SaveAddressesResult.Failure>();
    }

    [Fact]
    public async Task SaveAddressesAsync_EmptyStreetLine2_SendsNullInRequest()
    {
        string? capturedBody = null;
        var handler = new FakeHttpMessageHandler(HttpStatusCode.NoContent, body => capturedBody = body);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://test") };
        var service = new Step2Service(httpClient);

        var form = ValidForm();
        form.ShippingStreetLine2 = "   ";

        await service.SaveAddressesAsync(1, form, TestContext.Current.CancellationToken);

        capturedBody.ShouldNotBeNull();
        capturedBody.ShouldContain("\"streetLine2\":null");
    }

    private class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;
        private readonly Action<string>? _bodyCapture;

        public FakeHttpMessageHandler(HttpStatusCode statusCode, Action<string>? bodyCapture = null)
        {
            _statusCode = statusCode;
            _bodyCapture = bodyCapture;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (_bodyCapture is not null && request.Content is not null)
            {
                var body = await request.Content.ReadAsStringAsync(cancellationToken);
                _bodyCapture(body);
            }

            return new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent("{}", Encoding.UTF8, "application/json")
            };
        }
    }
}