using OniBusExpress.Application.DTOs;
using OniBusExpress.Domain.Repositories;

namespace OniBusExpress.Application.Services;

public sealed class RouteAppService : IRouteAppService
{
    private readonly IRouteRepository _routeRepository;

    public RouteAppService(IRouteRepository routeRepository)
    {
        _routeRepository = routeRepository;
    }

    public async Task<IReadOnlyList<RouteDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var routes = await _routeRepository.GetAllAsync(cancellationToken);

        return routes
            .Select(r => new RouteDto(r.Id, r.Origin, r.Destination, r.EstimatedDuration))
            .ToList();
    }
}
