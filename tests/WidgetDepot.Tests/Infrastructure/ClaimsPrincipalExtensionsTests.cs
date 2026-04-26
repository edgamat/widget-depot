using System.Security.Claims;

using Shouldly;

namespace WidgetDepot.Tests.Infrastructure;

public class ClaimsPrincipalExtensionsTests
{
    private static ClaimsPrincipal CreateUser(string? nameIdentifier)
    {
        var claims = nameIdentifier is null
            ? []
            : new[] { new Claim(ClaimTypes.NameIdentifier, nameIdentifier) };
        return new ClaimsPrincipal(new ClaimsIdentity(claims));
    }

    [Fact]
    public void TryGetCustomerId_ValidNameIdentifierClaim_ReturnsTrueAndCorrectId()
    {
        var user = CreateUser("42");

        var result = user.TryGetCustomerId(out var customerId);

        result.ShouldBeTrue();
        customerId.ShouldBe(42);
    }

    [Fact]
    public void TryGetCustomerId_MissingClaim_ReturnsFalseAndZero()
    {
        var user = CreateUser(null);

        var result = user.TryGetCustomerId(out var customerId);

        result.ShouldBeFalse();
        customerId.ShouldBe(0);
    }

    [Fact]
    public void TryGetCustomerId_NonNumericClaim_ReturnsFalseAndZero()
    {
        var user = CreateUser("not-a-number");

        var result = user.TryGetCustomerId(out var customerId);

        result.ShouldBeFalse();
        customerId.ShouldBe(0);
    }
}