using System.Net;
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
    public async Task GetProfileAsync_SuccessResponse_ReturnsSuccessWithProfileData()
    {
        var service = CreateService(HttpStatusCode.OK, """{"firstName":"Jane","lastName":"Doe","email":"jane@example.com"}""");

        var result = await service.GetProfileAsync(1, TestContext.Current.CancellationToken);

        var success = result.ShouldBeOfType<ProfileGetResult.Success>();
        success.Profile.FirstName.ShouldBe("Jane");
        success.Profile.LastName.ShouldBe("Doe");
        success.Profile.Email.ShouldBe("jane@example.com");
    }

    [Fact]
    public async Task GetProfileAsync_ServerError_ReturnsFailure()
    {
        var service = CreateService(HttpStatusCode.InternalServerError, "{}");

        var result = await service.GetProfileAsync(1, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<ProfileGetResult.Failure>();
    }

    [Fact]
    public async Task GetProfileAsync_NotFound_ReturnsFailure()
    {
        var service = CreateService(HttpStatusCode.NotFound, "{}");

        var result = await service.GetProfileAsync(1, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<ProfileGetResult.Failure>();
    }

    [Fact]
    public async Task UpdateProfileAsync_SuccessResponse_ReturnsSuccessWithUpdatedProfile()
    {
        var service = CreateService(HttpStatusCode.OK, """{"firstName":"Janet","lastName":"Smith","email":"janet@example.com"}""");
        var form = new ProfileFormModel { FirstName = "Janet", LastName = "Smith", Email = "janet@example.com" };

        var result = await service.UpdateProfileAsync(1, form, TestContext.Current.CancellationToken);

        var success = result.ShouldBeOfType<ProfileUpdateResult.Success>();
        success.Profile.FirstName.ShouldBe("Janet");
        success.Profile.LastName.ShouldBe("Smith");
        success.Profile.Email.ShouldBe("janet@example.com");
    }

    [Fact]
    public async Task UpdateProfileAsync_ConflictWithEmailAlreadyRegistered_ReturnsDuplicateEmail()
    {
        var body = """{"errorCode":"EmailAlreadyRegistered"}""";
        var service = CreateService(HttpStatusCode.Conflict, body);
        var form = new ProfileFormModel { FirstName = "Jane", LastName = "Doe", Email = "other@example.com" };

        var result = await service.UpdateProfileAsync(1, form, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<ProfileUpdateResult.DuplicateEmail>();
    }

    [Fact]
    public async Task UpdateProfileAsync_ConflictWithOtherErrorCode_ReturnsFailure()
    {
        var body = """{"errorCode":"OtherError"}""";
        var service = CreateService(HttpStatusCode.Conflict, body);
        var form = new ProfileFormModel { FirstName = "Jane", LastName = "Doe", Email = "jane@example.com" };

        var result = await service.UpdateProfileAsync(1, form, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<ProfileUpdateResult.Failure>();
    }

    [Fact]
    public async Task UpdateProfileAsync_ServerError_ReturnsFailure()
    {
        var service = CreateService(HttpStatusCode.InternalServerError, "{}");
        var form = new ProfileFormModel { FirstName = "Jane", LastName = "Doe", Email = "jane@example.com" };

        var result = await service.UpdateProfileAsync(1, form, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<ProfileUpdateResult.Failure>();
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
    public void ProfileFormModel_EmailRequired_ValidationFails(string? email)
    {
        var form = new ProfileFormModel { FirstName = "Jane", LastName = "Doe", Email = email! };

        var results = ValidateModel(form);

        results.ShouldContain(r => r.MemberNames.Contains(nameof(ProfileFormModel.Email)));
    }

    [Fact]
    public void ProfileFormModel_InvalidEmailFormat_ValidationFails()
    {
        var form = new ProfileFormModel { FirstName = "Jane", LastName = "Doe", Email = "not-an-email" };

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