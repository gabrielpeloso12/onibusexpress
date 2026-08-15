using Microsoft.AspNetCore.Mvc;
using OniBusExpress.Application.DTOs;
using OniBusExpress.Application.Services;

namespace OniBusExpress.Api.Controllers;

/// <summary>Criação, consulta e cancelamento de reservas de passagem.</summary>
[ApiController]
[Route("reservas")]
[Produces("application/json")]
public sealed class BookingsController : ControllerBase
{
    private readonly IBookingAppService _bookingAppService;

    public BookingsController(IBookingAppService bookingAppService)
    {
        _bookingAppService = bookingAppService;
    }

    /// <summary>Cria uma reserva para um assento de uma viagem.</summary>
    /// <response code="201">Reserva criada com sucesso.</response>
    /// <response code="400">Dados inválidos (ex.: CPF ou assento inválidos).</response>
    /// <response code="404">A viagem informada não existe.</response>
    /// <response code="409">O assento já está ocupado ou a viagem já foi realizada.</response>
    [HttpPost]
    [ProducesResponseType(typeof(BookingResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BookingResponseDto>> Create(
        [FromBody] CreateBookingRequest request,
        CancellationToken cancellationToken)
    {
        var booking = await _bookingAppService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetByCode), new { codigo = booking.ReservationCode }, booking);
    }

    /// <summary>Consulta uma reserva pelo código gerado (ex.: ABC-12345).</summary>
    /// <response code="200">Reserva encontrada.</response>
    /// <response code="404">Nenhuma reserva foi encontrada com o código informado.</response>
    [HttpGet("{codigo}")]
    [ProducesResponseType(typeof(BookingResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookingResponseDto>> GetByCode(string codigo, CancellationToken cancellationToken)
    {
        var booking = await _bookingAppService.GetByCodeAsync(codigo, cancellationToken);
        return booking is null ? NotFound() : Ok(booking);
    }

    /// <summary>Cancela uma reserva. Só é permitido até 2 horas antes da partida.</summary>
    /// <response code="204">Reserva cancelada com sucesso.</response>
    /// <response code="404">Nenhuma reserva foi encontrada com o código informado.</response>
    /// <response code="409">O prazo de cancelamento (2h antes da partida) já expirou, ou a reserva já está cancelada.</response>
    [HttpDelete("{codigo}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancel(string codigo, CancellationToken cancellationToken)
    {
        var cancelled = await _bookingAppService.CancelAsync(codigo, cancellationToken);
        return cancelled ? NoContent() : NotFound();
    }
}
