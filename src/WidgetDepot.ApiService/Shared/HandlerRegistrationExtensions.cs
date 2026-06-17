using System.Reflection;

namespace WidgetDepot.ApiService.Shared;

public static class HandlerRegistrationExtensions
{
    public static IServiceCollection AddRequestHandlers(this IServiceCollection services, params Assembly[] assemblies)
    {
        var handlerInterface = typeof(IRequestHandler<,>);

        if (assemblies.Length == 0)
        {
            assemblies = [Assembly.GetExecutingAssembly()];
        }

        var typesThatImplementAHandler = assemblies.SelectMany(a => a.GetTypes())
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .Select(t => new
            {
                Implementation = t,
                HandlerInterfaces = t.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterface)
            })
            .Where(x => x.HandlerInterfaces.Any());

        foreach (var type in typesThatImplementAHandler)
        {
            foreach (var @interface in type.HandlerInterfaces)
            {
                services.AddScoped(@interface, type.Implementation);
            }
        }

        return services;
    }
}