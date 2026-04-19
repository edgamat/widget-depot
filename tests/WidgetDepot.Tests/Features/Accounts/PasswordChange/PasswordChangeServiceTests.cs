using System.Text;

using Shouldly;

using WidgetDepot.Web.Features.Accounts.PasswordChange;

namespace WidgetDepot.Tests.Features.Accounts.PasswordChange;

public class PasswordChangeServiceTests
{
    private static PasswordChangeService CreateService(HttpStatusCode statusCode, string responseBody)
    {
        var handler = new FakeHttpMessageHandler(statusCode, responseBody);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://test") };
        return new PasswordChangeService(httpClient);
    }

    [Fact]
    public async Task ChangeAsync_SuccessResponse_ReturnsSuccess()
    {
        var service = CreateService(HttpStatusCode.NoContent, "");
        var form = new PasswordChangeFormModel
        {
            CurrentPassword = "OldPass1",
            NewPassword = "NewPass1",
            ConfirmNewPassword = "NewPass1"
        };

        var result = await service.ChangeAsync(form, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<ChangePasswordResult.Success>();
    }

    [Fact]
    public async Task ChangeAsync_ConflictWithIncorrectPassword_ReturnsIncorrectPassword()
    {
        var body = """{"errorCode": "IncorrectPassword"}""";
        var service = CreateService(HttpStatusCode.Conflict, body);
        var form = new PasswordChangeFormModel
        {
            CurrentPassword = "WrongPass",
            NewPassword = "NewPass1",
            ConfirmNewPassword = "NewPass1"
        };

        var result = await service.ChangeAsync(form, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<ChangePasswordResult.IncorrectPassword>();
    }

    [Fact]
    public async Task ChangeAsync_ConflictWithOtherErrorCode_ReturnsFailure()
    {
        var body = """{"errorCode": "OtherError"}""";
        var service = CreateService(HttpStatusCode.Conflict, body);
        var form = new PasswordChangeFormModel
        {
            CurrentPassword = "OldPass1",
            NewPassword = "NewPass1",
            ConfirmNewPassword = "NewPass1"
        };

        var result = await service.ChangeAsync(form, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<ChangePasswordResult.Failure>();
    }

    [Fact]
    public async Task ChangeAsync_ServerError_ReturnsFailure()
    {
        var service = CreateService(HttpStatusCode.InternalServerError, "{}");
        var form = new PasswordChangeFormModel
        {
            CurrentPassword = "OldPass1",
            NewPassword = "NewPass1",
            ConfirmNewPassword = "NewPass1"
        };

        var result = await service.ChangeAsync(form, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<ChangePasswordResult.Failure>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null!)]
    public void PasswordChangeFormModel_CurrentPasswordRequired_ValidationFails(string? currentPassword)
    {
        var form = new PasswordChangeFormModel
        {
            CurrentPassword = currentPassword!,
            NewPassword = "NewPass1",
            ConfirmNewPassword = "NewPass1"
        };

        var results = ValidateModel(form);

        results.ShouldContain(r => r.MemberNames.Contains(nameof(PasswordChangeFormModel.CurrentPassword)));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null!)]
    public void PasswordChangeFormModel_NewPasswordRequired_ValidationFails(string? newPassword)
    {
        var form = new PasswordChangeFormModel
        {
            CurrentPassword = "OldPass1",
            NewPassword = newPassword!,
            ConfirmNewPassword = "NewPass1"
        };

        var results = ValidateModel(form);

        results.ShouldContain(r => r.MemberNames.Contains(nameof(PasswordChangeFormModel.NewPassword)));
    }

    [Fact]
    public void PasswordChangeFormModel_NewPasswordTooShort_ValidationFails()
    {
        var form = new PasswordChangeFormModel
        {
            CurrentPassword = "OldPass1",
            NewPassword = "short",
            ConfirmNewPassword = "short"
        };

        var results = ValidateModel(form);

        results.ShouldContain(r => r.MemberNames.Contains(nameof(PasswordChangeFormModel.NewPassword)));
    }

    [Fact]
    public void PasswordChangeFormModel_ConfirmPasswordMismatch_ValidationFails()
    {
        var form = new PasswordChangeFormModel
        {
            CurrentPassword = "OldPass1",
            NewPassword = "NewPass1",
            ConfirmNewPassword = "Different1"
        };

        var results = ValidateModel(form);

        results.ShouldContain(r => r.MemberNames.Contains(nameof(PasswordChangeFormModel.ConfirmNewPassword)));
    }

    [Fact]
    public void PasswordChangeFormModel_ValidData_PassesValidation()
    {
        var form = new PasswordChangeFormModel
        {
            CurrentPassword = "OldPass1",
            NewPassword = "NewPass1",
            ConfirmNewPassword = "NewPass1"
        };

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