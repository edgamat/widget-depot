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
    public async Task GetDraftOrderAsync_OkResponse_ReturnsSuccessWithOrder()
    {
        var json = """{"id":1,"status":"Draft","items":[],"shippingAddress":null,"billingAddress":null}""";
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, responseContent: json);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://test") };
        var service = new Step2Service(httpClient);

        var result = await service.GetDraftOrderAsync(1, TestContext.Current.CancellationToken);

        var success = result.ShouldBeOfType<GetDraftOrderResult.Success>();
        success.Order.Id.ShouldBe(1);
        success.Order.Status.ShouldBe("Draft");
        success.Order.Items.ShouldBeEmpty();
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

        var result = await service.GetDraftOrderAsync(1, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<GetDraftOrderResult.Forbidden>();
    }

    [Fact]
    public async Task GetDraftOrderAsync_ServerErrorResponse_ReturnsFailure()
    {
        var service = CreateService(HttpStatusCode.InternalServerError);

        var result = await service.GetDraftOrderAsync(1, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<GetDraftOrderResult.Failure>();
    }

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

    [Fact]
    public async Task GetProfileAddressesAsync_OkResponseWithShippingAddress_ReturnsSuccessWithShippingAddress()
    {
        var json = """{"shippingAddress":{"recipientName":"Alice Smith","streetLine1":"123 Main St","streetLine2":null,"city":"Springfield","state":"IL","zipCode":"62701"},"billingAddress":null}""";
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, responseContent: json);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://test") };
        var service = new Step2Service(httpClient);

        var result = await service.GetProfileAddressesAsync(TestContext.Current.CancellationToken);

        var success = result.ShouldBeOfType<GetProfileAddressesResult.Success>();
        success.Profile.ShippingAddress.ShouldNotBeNull();
        success.Profile.ShippingAddress.RecipientName.ShouldBe("Alice Smith");
        success.Profile.ShippingAddress.StreetLine1.ShouldBe("123 Main St");
        success.Profile.BillingAddress.ShouldBeNull();
    }

    [Fact]
    public async Task GetProfileAddressesAsync_OkResponseWithNoAddresses_ReturnsSuccessWithNullAddresses()
    {
        var json = """{"shippingAddress":null,"billingAddress":null}""";
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, responseContent: json);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://test") };
        var service = new Step2Service(httpClient);

        var result = await service.GetProfileAddressesAsync(TestContext.Current.CancellationToken);

        var success = result.ShouldBeOfType<GetProfileAddressesResult.Success>();
        success.Profile.ShippingAddress.ShouldBeNull();
        success.Profile.BillingAddress.ShouldBeNull();
    }

    [Fact]
    public async Task GetProfileAddressesAsync_NonSuccessResponse_ReturnsFailure()
    {
        var service = CreateService(HttpStatusCode.InternalServerError);

        var result = await service.GetProfileAddressesAsync(TestContext.Current.CancellationToken);

        result.ShouldBeOfType<GetProfileAddressesResult.Failure>();
    }

    private class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;
        private readonly Action<string>? _bodyCapture;
        private readonly string _responseContent;

        public FakeHttpMessageHandler(HttpStatusCode statusCode, Action<string>? bodyCapture = null, string? responseContent = null)
        {
            _statusCode = statusCode;
            _bodyCapture = bodyCapture;
            _responseContent = responseContent ?? "{}";
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
                Content = new StringContent(_responseContent, Encoding.UTF8, "application/json")
            };
        }
    }
}