using Microsoft.AspNetCore.Mvc;
using OniBusExpress.Application.DTOs;
using OniBusExpress.Application.Services;

namespace OniBusExpress.Api.Controllers;

/// <summary>Viagens agendadas e disponibilidade de assentos.</summary>
[ApiController]
[Route("viagens")]
[Produces("application/json")]
public sealed class TripsController : ControllerBase
{
    private readonly ITripAppService _tripAppService;

    public TripsController(ITripAppService tripAppService)
    {
        _tripAppService = tripAppService;
    }

    /// <summary>Busca viagens por origem, destino e/ou data de partida.</summary>
    /// <param name="origem">Filtro parcial e case-insensitive pela cidade de origem.</param>
    /// <param name="destino">Filtro parcial e case-insensitive pela cidade de destino.</param>
    /// <param name="data">Filtra viagens que partem nessa data (formato yyyy-MM-dd).</param>
    /// <param name="cancellationToken">Token de cancelamento da requisição.</param>
    /// <response code="200">Lista de viagens encontradas (pode ser vazia).</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TripSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TripSummaryDto>>> Search(
        [FromQuery] string? origem,
        [FromQuery] string? destino,
        [FromQuery] DateOnly? data,
        CancellationToken cancellationToken)
    {
        var trips = await _tripAppService.SearchAsync(origem, destino, data, cancellationToken);
        return Ok(trips);
    }

    /// <summary>Detalha uma viagem, incluindo o mapa de assentos livres/ocupados.</summary>
    /// <response code="200">Viagem encontrada.</response>
    /// <response code="404">Nenhuma viagem foi encontrada com o identificador informado.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TripDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TripDetailsDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var trip = await _tripAppService.GetByIdAsync(id, cancellationToken);
        return trip is null ? NotFound() : Ok(trip);
    }
}
