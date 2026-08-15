using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OniBusExpress.Application.Abstractions;
using OniBusExpress.Domain.Repositories;
using OniBusExpress.Infra.Common;
using OniBusExpress.Infra.Persistence;
using OniBusExpress.Infra.Repositories;
using OniBusExpress.Infra.Services;

namespace OniBusExpress.Infra;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("A connection string 'DefaultConnection' não foi configurada.");

        services.AddDbContext<OniBusExpressDbContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<IRouteRepository, RouteRepository>();
        services.AddScoped<ITripRepository, TripRepository>();
        services.AddScoped<IPassengerRepository, PassengerRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IReservationCodeGenerator, ReservationCodeGenerator>();
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();

        return services;
    }
}
