using System.ComponentModel.DataAnnotations;

namespace OniBusExpress.Application.DTOs;

/// <summary>Payload de requisição de <c>POST /reservas</c>.</summary>
public sealed class CreateBookingRequest
{
    [Required(ErrorMessage = "O nome do passageiro é obrigatório.")]
    [MaxLength(200)]
    public string PassengerName { get; init; } = default!;

    [Required(ErrorMessage = "O CPF do passageiro é obrigatório.")]
    public string PassengerCpf { get; init; } = default!;

    [Required(ErrorMessage = "O e-mail do passageiro é obrigatório.")]
    [EmailAddress(ErrorMessage = "O e-mail informado é inválido.")]
    public string PassengerEmail { get; init; } = default!;

    [Required(ErrorMessage = "A data de nascimento do passageiro é obrigatória.")]
    public DateOnly PassengerBirthDate { get; init; }

    [Required(ErrorMessage = "A viagem é obrigatória.")]
    public Guid TripId { get; init; }

    [Range(1, int.MaxValue, ErrorMessage = "O número do assento deve ser maior que zero.")]
    public int SeatNumber { get; init; }
}

/// <summary>Payload de resposta para criação, consulta e confirmação de cancelamento de reserva.</summary>
public sealed record BookingResponseDto(
    string ReservationCode,
    string Status,
    Guid TripId,
    string Origin,
    string Destination,
    DateTime DepartureDateTime,
    decimal BasePrice,
    int SeatNumber,
    string PassengerName,
    string PassengerCpf,
    string PassengerEmail,
    DateTime CreatedAtUtc,
    DateTime? CancelledAtUtc);
