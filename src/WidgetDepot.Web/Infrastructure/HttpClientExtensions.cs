using WidgetDepot.Web.Features.Accounts.ForcePasswordChange;
using WidgetDepot.Web.Features.Accounts.Login;
using WidgetDepot.Web.Features.Accounts.PasswordChange;
using WidgetDepot.Web.Features.Accounts.Profile;
using WidgetDepot.Web.Features.Accounts.Register;
using WidgetDepot.Web.Features.Admin.CatalogImport;
using WidgetDepot.Web.Features.Admin.Customers;
using WidgetDepot.Web.Features.Admin.Orders;
using WidgetDepot.Web.Features.Catalog;
using WidgetDepot.Web.Features.Orders.Create.Step1;
using WidgetDepot.Web.Features.Orders.Create.Step2;
using WidgetDepot.Web.Features.Orders.Create.Step3;
using WidgetDepot.Web.Features.Orders.Detail;
using WidgetDepot.Web.Features.Orders.History;
using WidgetDepot.Web.Features.Orders.List;
using WidgetDepot.Web.Features.Orders.Submit;
using WidgetDepot.Web.Features.ProblemReports.OrderLookup;

namespace WidgetDepot.Web.Infrastructure;

internal static class HttpClientExtensions
{
    private const string ApiServiceBaseAddress = "https+http://apiservice";

    public static IServiceCollection AddApiHttpClients(this IServiceCollection services)
    {
        services.AddHttpClient<CatalogService>(c => c.BaseAddress = new Uri(ApiServiceBaseAddress));
        services.AddHttpClient<CatalogImportService>(c => c.BaseAddress = new Uri(ApiServiceBaseAddress));
        services.AddHttpClient<RegisterService>(c => c.BaseAddress = new Uri(ApiServiceBaseAddress));
        services.AddHttpClient<LoginService>(c => c.BaseAddress = new Uri(ApiServiceBaseAddress));

        Action<HttpClient> withBase = c => c.BaseAddress = new Uri(ApiServiceBaseAddress);
        services.AddHttpClient<CustomerListService>(withBase).AddHttpMessageHandler<CookieForwardingHandler>();
        services.AddHttpClient<ProfileService>(withBase).AddHttpMessageHandler<CookieForwardingHandler>();
        services.AddHttpClient<PasswordChangeService>(withBase).AddHttpMessageHandler<CookieForwardingHandler>();
        services.AddHttpClient<ForcePasswordChangeService>(withBase).AddHttpMessageHandler<CookieForwardingHandler>();
        services.AddHttpClient<Step1Service>(withBase).AddHttpMessageHandler<CookieForwardingHandler>();
        services.AddHttpClient<Step2Service>(withBase).AddHttpMessageHandler<CookieForwardingHandler>();
        services.AddHttpClient<Step3Service>(withBase).AddHttpMessageHandler<CookieForwardingHandler>();
        services.AddHttpClient<ListService>(withBase).AddHttpMessageHandler<CookieForwardingHandler>();
        services.AddHttpClient<Step4Service>(withBase).AddHttpMessageHandler<CookieForwardingHandler>();
        services.AddHttpClient<HistoryService>(withBase).AddHttpMessageHandler<CookieForwardingHandler>();
        services.AddHttpClient<DetailService>(withBase).AddHttpMessageHandler<CookieForwardingHandler>();
        services.AddHttpClient<AdminOrderLookupService>(withBase).AddHttpMessageHandler<CookieForwardingHandler>();
        services.AddHttpClient<OrderLookupService>(withBase).AddHttpMessageHandler<CookieForwardingHandler>();

        return services;
    }
}