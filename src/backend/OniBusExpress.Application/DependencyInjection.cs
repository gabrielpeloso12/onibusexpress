using Microsoft.Extensions.DependencyInjection;
using OniBusExpress.Application.Services;

namespace OniBusExpress.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IRouteAppService, RouteAppService>();
        services.AddScoped<ITripAppService, TripAppService>();
        services.AddScoped<IBookingAppService, BookingAppService>();

        return services;
    }
}
