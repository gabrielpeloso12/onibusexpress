using Microsoft.AspNetCore.Mvc;
using OniBusExpress.Application.DTOs;
using OniBusExpress.Application.Services;

namespace OniBusExpress.Api.Controllers;

/// <summary>Rotas disponíveis para venda de passagens.</summary>
[ApiController]
[Route("rotas")]
[Produces("application/json")]
public sealed class RoutesController : ControllerBase
{
    private readonly IRouteAppService _routeAppService;

    public RoutesController(IRouteAppService routeAppService)
    {
        _routeAppService = routeAppService;
    }

    /// <summary>Lista todas as rotas cadastradas.</summary>
    /// <response code="200">Lista de rotas retornada com sucesso.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<RouteDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<RouteDto>>> GetAll(CancellationToken cancellationToken)
    {
        var routes = await _routeAppService.GetAllAsync(cancellationToken);
        return Ok(routes);
    }
}
