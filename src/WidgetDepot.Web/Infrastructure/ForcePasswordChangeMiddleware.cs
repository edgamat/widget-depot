namespace WidgetDepot.Web.Infrastructure;

public class ForcePasswordChangeMiddleware(RequestDelegate next)
{
    private static readonly string[] ExcludedPrefixes =
    [
        "/_blazor",
        "/_framework",
        "/lib",
    ];

    private static readonly string[] ExcludedExtensions =
    [
        ".css",
        ".js",
        ".png",
    ];

    private static readonly string[] ExcludedPaths =
    [
        "/accounts/force-password-change",
        "/accounts/do-signin",
        "/accounts/logout",
        "/accounts/login",
    ];

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true
            && context.User.HasClaim("MustChangePassword", "true"))
        {
            var path = context.Request.Path.Value ?? string.Empty;

            var isExcluded = Array.Exists(ExcludedPaths, p => path.Equals(p, StringComparison.OrdinalIgnoreCase))
                          || Array.Exists(ExcludedPrefixes, p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase))
                          || Array.Exists(ExcludedExtensions, e => path.EndsWith(e, StringComparison.OrdinalIgnoreCase));

            if (!isExcluded)
            {
                context.Response.Redirect("/accounts/force-password-change");
                return;
            }
        }

        await next(context);
    }
}