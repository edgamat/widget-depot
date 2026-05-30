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
        var body = """[{"id":1,"submittedAt":"2026-05-01T00:00:00","widgetCount":3,"shippingEstimate":12.50,"transmissionStatus":0,"transmissionStatusChangedAt":null}]""";
        var service = CreateService(HttpStatusCode.OK, body);

        var result = await service.GetRecentOrdersAsync(TestContext.Current.CancellationToken);

        var success = result.ShouldBeOfType<GetRecentOrdersResult.Success>();
        success.Orders.Count.ShouldBe(1);
        success.Orders[0].Id.ShouldBe(1);
        success.Orders[0].WidgetCount.ShouldBe(3);
        success.Orders[0].ShippingEstimate.ShouldBe(12.50m);
    }

    [Fact]
    public async Task GetRecentOrdersAsync_PendingTransmission_ReturnsPendingStatus()
    {
        var body = """[{"id":1,"submittedAt":"2026-05-01T00:00:00","widgetCount":1,"shippingEstimate":null,"transmissionStatus":0,"transmissionStatusChangedAt":null}]""";
        var service = CreateService(HttpStatusCode.OK, body);

        var result = await service.GetRecentOrdersAsync(TestContext.Current.CancellationToken);

        var success = result.ShouldBeOfType<GetRecentOrdersResult.Success>();
        success.Orders[0].TransmissionStatus.ShouldBe(TransmissionStatus.Pending);
        success.Orders[0].TransmissionStatusChangedAt.ShouldBeNull();
    }

    [Fact]
    public async Task GetRecentOrdersAsync_TransmittedOrder_ReturnsStatusAndTimestamp()
    {
        var body = """[{"id":1,"submittedAt":"2026-05-01T00:00:00","widgetCount":1,"shippingEstimate":null,"transmissionStatus":1,"transmissionStatusChangedAt":"2026-05-02T08:30:00"}]""";
        var service = CreateService(HttpStatusCode.OK, body);

        var result = await service.GetRecentOrdersAsync(TestContext.Current.CancellationToken);

        var success = result.ShouldBeOfType<GetRecentOrdersResult.Success>();
        success.Orders[0].TransmissionStatus.ShouldBe(TransmissionStatus.Transmitted);
        success.Orders[0].TransmissionStatusChangedAt.ShouldBe(new DateTime(2026, 5, 2, 8, 30, 0));
    }

    [Fact]
    public async Task GetRecentOrdersAsync_FailedOrder_ReturnsFailedStatus()
    {
        var body = """[{"id":1,"submittedAt":"2026-05-01T00:00:00","widgetCount":1,"shippingEstimate":null,"transmissionStatus":2,"transmissionStatusChangedAt":"2026-05-02T08:30:00"}]""";
        var service = CreateService(HttpStatusCode.OK, body);

        var result = await service.GetRecentOrdersAsync(TestContext.Current.CancellationToken);

        var success = result.ShouldBeOfType<GetRecentOrdersResult.Success>();
        success.Orders[0].TransmissionStatus.ShouldBe(TransmissionStatus.Failed);
    }

    [Fact]
    public async Task GetRecentOrdersAsync_MissingOrder_ReturnsMissingStatus()
    {
        var body = """[{"id":1,"submittedAt":"2026-05-01T00:00:00","widgetCount":1,"shippingEstimate":null,"transmissionStatus":3,"transmissionStatusChangedAt":"2026-05-02T08:30:00"}]""";
        var service = CreateService(HttpStatusCode.OK, body);

        var result = await service.GetRecentOrdersAsync(TestContext.Current.CancellationToken);

        var success = result.ShouldBeOfType<GetRecentOrdersResult.Success>();
        success.Orders[0].TransmissionStatus.ShouldBe(TransmissionStatus.Missing);
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

    [Fact]
    public async Task RecreateOrderAsync_SuccessTransmitted_ReturnsTransmitted()
    {
        var body = """{"newStatus":1,"statusChangedAt":"2026-05-30T10:00:00","errorMessage":null}""";
        var service = CreateService(HttpStatusCode.OK, body);

        var result = await service.RecreateOrderAsync(1, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<RecreateResult.Transmitted>();
    }

    [Fact]
    public async Task RecreateOrderAsync_SuccessWithFailedStatus_ReturnsFailedWithErrorMessage()
    {
        var body = """{"newStatus":2,"statusChangedAt":"2026-05-30T10:00:00","errorMessage":"FTP transmission failed."}""";
        var service = CreateService(HttpStatusCode.OK, body);

        var result = await service.RecreateOrderAsync(1, TestContext.Current.CancellationToken);

        var failed = result.ShouldBeOfType<RecreateResult.Failed>();
        failed.ErrorMessage.ShouldBe("FTP transmission failed.");
    }

    [Fact]
    public async Task RecreateOrderAsync_ServerError_ReturnsFailure()
    {
        var service = CreateService(HttpStatusCode.InternalServerError, "{}");

        var result = await service.RecreateOrderAsync(1, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<RecreateResult.Failure>();
    }

    [Fact]
    public async Task RecreateOrderAsync_Conflict_ReturnsFailure()
    {
        var service = CreateService(HttpStatusCode.Conflict, "{}");

        var result = await service.RecreateOrderAsync(1, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<RecreateResult.Failure>();
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