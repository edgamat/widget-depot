using Microsoft.Net.Http.Headers;

namespace WidgetDepot.Web.Infrastructure;

public class CookieForwardingHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var cookie = httpContextAccessor.HttpContext?.Request.Headers[HeaderNames.Cookie].ToString();

        if (!string.IsNullOrEmpty(cookie))
        {
            request.Headers.Remove(HeaderNames.Cookie);
            request.Headers.TryAddWithoutValidation(HeaderNames.Cookie, cookie);
        }

        return base.SendAsync(request, cancellationToken);
    }
}