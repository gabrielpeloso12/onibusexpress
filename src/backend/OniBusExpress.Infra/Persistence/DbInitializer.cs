using Microsoft.EntityFrameworkCore;
using OniBusExpress.Domain.Entities;
using DomainRoute = OniBusExpress.Domain.Entities.Route;

namespace OniBusExpress.Infra.Persistence;

/// <summary>Aplica as migrations pendentes e popula dados de exemplo (rotas, viagens) no startup.</summary>
public static class DbInitializer
{
    /// <summary>Aplica as migrations pendentes e depois popula os dados de exemplo. Usado pela API no startup.</summary>
    public static async Task InitializeAsync(OniBusExpressDbContext dbContext)
    {
        await dbContext.Database.MigrateAsync();
        await SeedAsync(dbContext);
    }

    /// <summary>Popula os dados de exemplo sem mexer em migrations. Usado pelos testes de integração contra um schema in-memory novo.</summary>
    public static async Task SeedAsync(OniBusExpressDbContext dbContext)
    {
        if (!await dbContext.Routes.AnyAsync())
        {
            var saoPauloRio = new DomainRoute("São Paulo", "Rio de Janeiro", TimeSpan.FromHours(6));
            var saoPauloBeloHorizonte = new DomainRoute("São Paulo", "Belo Horizonte", TimeSpan.FromHours(8));
            var rioSaoPaulo = new DomainRoute("Rio de Janeiro", "São Paulo", TimeSpan.FromHours(6));
            var curitibaSaoPaulo = new DomainRoute("Curitiba", "São Paulo", TimeSpan.FromHours(7));

            dbContext.Routes.AddRange(saoPauloRio, saoPauloBeloHorizonte, rioSaoPaulo, curitibaSaoPaulo);

            var today = DateTime.UtcNow.Date;
            dbContext.Trips.AddRange(
                new Trip(saoPauloRio.Id, today.AddDays(1).AddHours(8), 120.00m, 40),
                new Trip(saoPauloRio.Id, today.AddDays(1).AddHours(22), 135.50m, 40),
                new Trip(saoPauloRio.Id, today.AddDays(2).AddHours(8), 120.00m, 40),
                new Trip(saoPauloBeloHorizonte.Id, today.AddDays(1).AddHours(9), 98.00m, 36),
                new Trip(rioSaoPaulo.Id, today.AddDays(1).AddHours(10), 120.00m, 40),
                new Trip(curitibaSaoPaulo.Id, today.AddDays(3).AddHours(23), 89.90m, 44));
        }

        await dbContext.SaveChangesAsync();
    }
}
