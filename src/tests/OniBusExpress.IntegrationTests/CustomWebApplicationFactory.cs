using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using OniBusExpress.Infra.Persistence;

namespace OniBusExpress.IntegrationTests;

/// <summary>
/// Inicializa o pipeline real da API (controllers, middleware de exceções) contra um
/// banco SQLite in-memory em vez do PostgreSQL, para que os testes de integração não dependam de
/// serviços externos. A conexão é mantida aberta durante todo o ciclo de vida da factory, já que bancos
/// SQLite ":memory:" são destruídos quando sua única conexão é fechada.
/// </summary>
public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _connection.Open();

        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<OniBusExpressDbContext>>();
            services.AddDbContext<OniBusExpressDbContext>(options => options.UseSqlite(_connection));
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        using var scope = host.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OniBusExpressDbContext>();

        dbContext.Database.EnsureCreated();
        DbInitializer.SeedAsync(dbContext).GetAwaiter().GetResult();

        return host;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
            _connection.Dispose();
    }
}
