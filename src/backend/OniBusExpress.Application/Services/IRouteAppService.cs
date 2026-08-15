using OniBusExpress.Application.DTOs;

namespace OniBusExpress.Application.Services;

public interface IRouteAppService
{
    Task<IReadOnlyList<RouteDto>> GetAllAsync(CancellationToken cancellationToken = default);
}
