#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System.Security.Claims;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public static class ClaimsPrincipalExtensions
{
    public static bool TryGetCustomerId(this ClaimsPrincipal user, out int customerId)
    {
        var claim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(claim, out customerId);
    }
}