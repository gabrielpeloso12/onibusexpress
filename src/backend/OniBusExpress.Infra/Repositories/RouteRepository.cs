using Microsoft.EntityFrameworkCore;
using OniBusExpress.Domain.Repositories;
using OniBusExpress.Infra.Persistence;
using DomainRoute = OniBusExpress.Domain.Entities.Route;

namespace OniBusExpress.Infra.Repositories;

public sealed class RouteRepository : IRouteRepository
{
    private readonly OniBusExpressDbContext _dbContext;

    public RouteRepository(OniBusExpressDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<DomainRoute>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.Routes
            .AsNoTracking()
            .OrderBy(r => r.Origin)
            .ThenBy(r => r.Destination)
            .ToListAsync(cancellationToken);

    public async Task<DomainRoute?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _dbContext.Routes.AsNoTracking().SingleOrDefaultAsync(r => r.Id == id, cancellationToken);
}
