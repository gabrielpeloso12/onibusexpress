using OniBusExpress.Domain.Entities;

namespace OniBusExpress.Domain.Repositories;

public interface IRouteRepository
{
    Task<IReadOnlyList<Route>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Route?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
