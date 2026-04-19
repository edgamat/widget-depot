using System.Text;

using Shouldly;

using WidgetDepot.Web.Features.Accounts.Profile;

namespace WidgetDepot.Tests.Features.Accounts.Profile;

public class ProfileServiceTests
{
    private static ProfileService CreateService(HttpStatusCode statusCode, string responseBody)
    {
        var handler = new FakeHttpMessageHandler(statusCode, responseBody);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://test") };
        return new ProfileService(httpClient);
    }

    [Fact]
    public async Task LoadAsync_SuccessResponse_ReturnsSuccessWithProfile()
    {
        var service = CreateService(HttpStatusCode.OK, """{"firstName": "Jane", "lastName": "Doe", "email": "jane@example.com"}""");

        var result = await service.LoadAsync(TestContext.Current.CancellationToken);

        var success = result.ShouldBeOfType<LoadProfileResult.Success>();
        success.Profile.FirstName.ShouldBe("Jane");
        success.Profile.LastName.ShouldBe("Doe");
        success.Profile.Email.ShouldBe("jane@example.com");
    }

    [Fact]
    public async Task LoadAsync_NotFound_ReturnsFailure()
    {
        var service = CreateService(HttpStatusCode.NotFound, "{}");

        var result = await service.LoadAsync(TestContext.Current.CancellationToken);

        result.ShouldBeOfType<LoadProfileResult.Failure>();
    }

    [Fact]
    public async Task LoadAsync_ServerError_ReturnsFailure()
    {
        var service = CreateService(HttpStatusCode.InternalServerError, "{}");

        var result = await service.LoadAsync(TestContext.Current.CancellationToken);

        result.ShouldBeOfType<LoadProfileResult.Failure>();
    }

    [Fact]
    public async Task UpdateAsync_SuccessResponse_ReturnsSuccessWithProfile()
    {
        var service = CreateService(HttpStatusCode.OK, """{"firstName": "Jane", "lastName": "Smith", "email": "jane.smith@example.com"}""");
        var form = new ProfileFormModel { FirstName = "Jane", LastName = "Smith", Email = "jane.smith@example.com" };

        var result = await service.UpdateAsync(form, TestContext.Current.CancellationToken);

        var success = result.ShouldBeOfType<UpdateProfileResult.Success>();
        success.Profile.FirstName.ShouldBe("Jane");
        success.Profile.LastName.ShouldBe("Smith");
        success.Profile.Email.ShouldBe("jane.smith@example.com");
    }

    [Fact]
    public async Task UpdateAsync_ConflictWithEmailAlreadyRegistered_ReturnsDuplicateEmail()
    {
        var body = """{"errorCode": "EmailAlreadyRegistered"}""";
        var service = CreateService(HttpStatusCode.Conflict, body);
        var form = new ProfileFormModel { FirstName = "Jane", LastName = "Doe", Email = "jane@example.com" };

        var result = await service.UpdateAsync(form, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<UpdateProfileResult.DuplicateEmail>();
    }

    [Fact]
    public async Task UpdateAsync_ConflictWithOtherErrorCode_ReturnsFailure()
    {
        var body = """{"errorCode": "OtherError"}""";
        var service = CreateService(HttpStatusCode.Conflict, body);
        var form = new ProfileFormModel { FirstName = "Jane", LastName = "Doe", Email = "jane@example.com" };

        var result = await service.UpdateAsync(form, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<UpdateProfileResult.Failure>();
    }

    [Fact]
    public async Task UpdateAsync_ServerError_ReturnsFailure()
    {
        var service = CreateService(HttpStatusCode.InternalServerError, "{}");
        var form = new ProfileFormModel { FirstName = "Jane", LastName = "Doe", Email = "jane@example.com" };

        var result = await service.UpdateAsync(form, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<UpdateProfileResult.Failure>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null!)]
    public void ProfileFormModel_FirstNameRequired_ValidationFails(string? firstName)
    {
        var form = new ProfileFormModel { FirstName = firstName!, LastName = "Doe", Email = "jane@example.com" };

        var results = ValidateModel(form);

        results.ShouldContain(r => r.MemberNames.Contains(nameof(ProfileFormModel.FirstName)));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null!)]
    public void ProfileFormModel_LastNameRequired_ValidationFails(string? lastName)
    {
        var form = new ProfileFormModel { FirstName = "Jane", LastName = lastName!, Email = "jane@example.com" };

        var results = ValidateModel(form);

        results.ShouldContain(r => r.MemberNames.Contains(nameof(ProfileFormModel.LastName)));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null!)]
    [InlineData("not-an-email")]
    public void ProfileFormModel_EmailMustBeValid_ValidationFails(string? email)
    {
        var form = new ProfileFormModel { FirstName = "Jane", LastName = "Doe", Email = email! };

        var results = ValidateModel(form);

        results.ShouldContain(r => r.MemberNames.Contains(nameof(ProfileFormModel.Email)));
    }

    [Fact]
    public void ProfileFormModel_ValidData_PassesValidation()
    {
        var form = new ProfileFormModel { FirstName = "Jane", LastName = "Doe", Email = "jane@example.com" };

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