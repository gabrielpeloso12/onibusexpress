using Microsoft.EntityFrameworkCore;
using OniBusExpress.Domain.Entities;
using DomainRoute = OniBusExpress.Domain.Entities.Route;

namespace OniBusExpress.Infra.Persistence;

/// <summary>Aplica as migrations pendentes e popula dados de exemplo (rotas, viagens) no startup.</summary>
public static class DbInitializer
{
    private static readonly string[] BrazilianStates =
    {
        "Acre", "Alagoas", "Amapá", "Amazonas", "Bahia", "Ceará", "Espírito Santo", "Goiás",
        "Maranhão", "Mato Grosso", "Mato Grosso do Sul", "Minas Gerais", "Pará", "Paraíba",
        "Paraná", "Pernambuco", "Piauí", "Rio de Janeiro", "Rio Grande do Norte",
        "Rio Grande do Sul", "Rondônia", "Roraima", "Santa Catarina", "São Paulo", "Sergipe",
        "Tocantins"
    };

    /// <summary>Aplica as migrations pendentes e depois popula os dados de exemplo. Usado pela API no startup.</summary>
    public static async Task InitializeAsync(OniBusExpressDbContext dbContext)
    {
        await dbContext.Database.MigrateAsync();
        await SeedAsync(dbContext);
        await SeedStateTripsForAugustAsync(dbContext);
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

    /// <summary>
    /// Popula uma viagem por dia, em agosto de 2026, para cada combinação origem/destino entre os 26
    /// estados brasileiros (650 pares ordenados x 31 dias = 20.150 viagens) — dataset amplo para sempre
    /// haver opções de busca/agendamento. Roda só no startup real da API (via <see cref="InitializeAsync"/>);
    /// os testes de integração chamam <see cref="SeedAsync"/> diretamente e não pagam esse custo extra.
    /// Duração (10h), preço (R$ 150,00) e assentos (40) são valores fixos de placeholder — não há uma
    /// base de distância real entre estados aqui. Datas já passadas de agosto/2026 são inseridas mesmo
    /// assim (não ficam ocultas), mas deixam de ser agendáveis (regra `TripAlreadyDepartedException`).
    /// </summary>
    public static async Task SeedStateTripsForAugustAsync(OniBusExpressDbContext dbContext)
    {
        const string sentinelState = "Acre";
        if (await dbContext.Routes.AnyAsync(r => r.Origin == sentinelState))
            return;

        var pairs = new List<(string Origin, string Destination)>();
        foreach (var origin in BrazilianStates)
        {
            foreach (var destination in BrazilianStates)
            {
                if (origin != destination)
                    pairs.Add((origin, destination));
            }
        }

        var existingRouteIdsByPair = await dbContext.Routes
            .Select(r => new { r.Id, r.Origin, r.Destination })
            .ToDictionaryAsync(r => (r.Origin, r.Destination), r => r.Id);

        var estimatedDuration = TimeSpan.FromHours(10);
        const decimal basePrice = 150.00m;
        const int totalSeats = 40;

        var newRoutes = new List<DomainRoute>();
        var routeIdsByPair = new Dictionary<(string Origin, string Destination), Guid>();

        foreach (var pair in pairs)
        {
            if (existingRouteIdsByPair.TryGetValue(pair, out var existingId))
            {
                routeIdsByPair[pair] = existingId;
                continue;
            }

            var route = new DomainRoute(pair.Origin, pair.Destination, estimatedDuration);
            newRoutes.Add(route);
            routeIdsByPair[pair] = route.Id;
        }

        dbContext.Routes.AddRange(newRoutes);

        const int year = 2026;
        var daysInAugust = DateTime.DaysInMonth(year, 8);

        var trips = new List<Trip>(routeIdsByPair.Count * daysInAugust);
        foreach (var routeId in routeIdsByPair.Values)
        {
            for (var day = 1; day <= daysInAugust; day++)
            {
                var departure = new DateTime(year, 8, day, 8, 0, 0, DateTimeKind.Utc);
                trips.Add(new Trip(routeId, departure, basePrice, totalSeats));
            }
        }

        dbContext.Trips.AddRange(trips);
        await dbContext.SaveChangesAsync();
    }
}
