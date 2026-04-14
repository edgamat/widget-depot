using System.Text;

using Shouldly;

using WidgetDepot.Web.Features.Accounts.Register;

namespace WidgetDepot.Tests.Features.Accounts.Register;

public class RegisterServiceTests
{
    private static RegisterService CreateService(HttpStatusCode statusCode, string responseBody)
    {
        var handler = new FakeHttpMessageHandler(statusCode, responseBody);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://test") };
        return new RegisterService(httpClient);
    }

    [Fact]
    public async Task RegisterAsync_SuccessResponse_ReturnsSuccess()
    {
        var service = CreateService(HttpStatusCode.OK, """{"customerId": 1}""");
        var form = new RegisterFormModel
        {
            FirstName = "Jane",
            LastName = "Doe",
            Email = "jane@example.com",
            Password = "P@ssw0rd!",
            ConfirmPassword = "P@ssw0rd!"
        };

        var result = await service.RegisterAsync(form, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<RegisterResult.Success>();
    }

    [Fact]
    public async Task RegisterAsync_ConflictWithEmailAlreadyRegistered_ReturnsEmailAlreadyRegistered()
    {
        var body = """
            {
                "errorCode": "EmailAlreadyRegistered"
            }
            """;
        var service = CreateService(HttpStatusCode.Conflict, body);
        var form = new RegisterFormModel
        {
            FirstName = "Jane",
            LastName = "Doe",
            Email = "jane@example.com",
            Password = "P@ssw0rd!",
            ConfirmPassword = "P@ssw0rd!"
        };

        var result = await service.RegisterAsync(form, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<RegisterResult.EmailAlreadyRegistered>();
    }

    [Fact]
    public async Task RegisterAsync_ConflictWithOtherErrorCode_ReturnsFailure()
    {
        var body = """{"errorCode": "OtherError"}""";
        var service = CreateService(HttpStatusCode.Conflict, body);
        var form = new RegisterFormModel
        {
            FirstName = "Jane",
            LastName = "Doe",
            Email = "jane@example.com",
            Password = "P@ssw0rd!",
            ConfirmPassword = "P@ssw0rd!"
        };

        var result = await service.RegisterAsync(form, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<RegisterResult.Failure>();
    }

    [Fact]
    public async Task RegisterAsync_ServerError_ReturnsFailure()
    {
        var service = CreateService(HttpStatusCode.InternalServerError, "{}");
        var form = new RegisterFormModel
        {
            FirstName = "Jane",
            LastName = "Doe",
            Email = "jane@example.com",
            Password = "P@ssw0rd!",
            ConfirmPassword = "P@ssw0rd!"
        };

        var result = await service.RegisterAsync(form, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<RegisterResult.Failure>();
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