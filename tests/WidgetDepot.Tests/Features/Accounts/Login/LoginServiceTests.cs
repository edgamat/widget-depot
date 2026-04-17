using System.Text;

using Shouldly;

using WidgetDepot.Web.Features.Accounts.Login;

namespace WidgetDepot.Tests.Features.Accounts.Login;

public class LoginServiceTests
{
    private static LoginService CreateService(HttpStatusCode statusCode, string responseBody)
    {
        var handler = new FakeHttpMessageHandler(statusCode, responseBody);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://test") };
        return new LoginService(httpClient);
    }

    [Fact]
    public async Task LoginAsync_SuccessResponse_ReturnsSuccessWithCustomerInfo()
    {
        var service = CreateService(HttpStatusCode.OK, """{"customerId": 42, "email": "jane@example.com", "firstName": "Jane"}""");
        var form = new LoginFormModel { Email = "jane@example.com", Password = "P@ssw0rd!" };

        var result = await service.LoginAsync(form, TestContext.Current.CancellationToken);

        var success = result.ShouldBeOfType<LoginResult.Success>();
        success.Customer.CustomerId.ShouldBe(42);
        success.Customer.Email.ShouldBe("jane@example.com");
        success.Customer.FirstName.ShouldBe("Jane");
    }

    [Fact]
    public async Task LoginAsync_UnauthorizedWithInvalidCredentials_ReturnsInvalidCredentials()
    {
        var body = """{"errorCode": "InvalidCredentials"}""";
        var service = CreateService(HttpStatusCode.Unauthorized, body);
        var form = new LoginFormModel { Email = "jane@example.com", Password = "wrong" };

        var result = await service.LoginAsync(form, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<LoginResult.InvalidCredentials>();
    }

    [Fact]
    public async Task LoginAsync_UnauthorizedWithOtherErrorCode_ReturnsFailure()
    {
        var body = """{"errorCode": "OtherError"}""";
        var service = CreateService(HttpStatusCode.Unauthorized, body);
        var form = new LoginFormModel { Email = "jane@example.com", Password = "wrong" };

        var result = await service.LoginAsync(form, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<LoginResult.Failure>();
    }

    [Fact]
    public async Task LoginAsync_ServerError_ReturnsFailure()
    {
        var service = CreateService(HttpStatusCode.InternalServerError, "{}");
        var form = new LoginFormModel { Email = "jane@example.com", Password = "P@ssw0rd!" };

        var result = await service.LoginAsync(form, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<LoginResult.Failure>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null!)]
    public void LoginFormModel_EmailRequired_ValidationFails(string? email)
    {
        var form = new LoginFormModel { Email = email!, Password = "P@ssw0rd!" };

        var results = ValidateModel(form);

        results.ShouldContain(r => r.MemberNames.Contains(nameof(LoginFormModel.Email)));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null!)]
    public void LoginFormModel_PasswordRequired_ValidationFails(string? password)
    {
        var form = new LoginFormModel { Email = "jane@example.com", Password = password! };

        var results = ValidateModel(form);

        results.ShouldContain(r => r.MemberNames.Contains(nameof(LoginFormModel.Password)));
    }

    [Fact]
    public void LoginFormModel_ValidData_PassesValidation()
    {
        var form = new LoginFormModel { Email = "jane@example.com", Password = "P@ssw0rd!" };

        var results = ValidateModel(form);

        results.ShouldBeEmpty();
    }

    private static List<System.ComponentModel.DataAnnotations.ValidationResult> ValidateModel(object model)
    {
        var results = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
        var context = new System.ComponentModel.DataAnnotations.ValidationContext(model);
        System.ComponentModel.DataAnnotations.Validator.TryValidateObject(model, context, results, validateAllProperties: true);
        return results;
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