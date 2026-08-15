using OniBusExpress.Domain.Common;
using OniBusExpress.Domain.Enums;
using OniBusExpress.Domain.Exceptions;

namespace OniBusExpress.Domain.Entities;

/// <summary>Uma passagem confirmada ou cancelada para um assento em uma <see cref="Trip"/>.</summary>
public class Booking : Entity
{
    public Guid TripId { get; private set; }
    public Trip? Trip { get; private set; }
    public Guid PassengerId { get; private set; }
    public Passenger? Passenger { get; private set; }
    public int SeatNumber { get; private set; }
    public string ReservationCode { get; private set; } = default!;
    public BookingStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? CancelledAtUtc { get; private set; }

    /// <summary>Até quanto tempo antes da partida uma reserva ainda pode ser cancelada.</summary>
    public static readonly TimeSpan CancellationWindow = TimeSpan.FromHours(2);

    private Booking()
    {
        // Construtor exigido pelo EF Core
    }

    private Booking(Guid tripId, Guid passengerId, int seatNumber, string reservationCode, DateTime nowUtc)
    {
        TripId = tripId;
        PassengerId = passengerId;
        SeatNumber = seatNumber;
        ReservationCode = reservationCode;
        Status = BookingStatus.Confirmed;
        CreatedAtUtc = nowUtc;
    }

    /// <summary>
    /// Cria uma reserva confirmada. Quem chama já deve ter validado o assento/viagem via
    /// <see cref="Trip.EnsureCanBeBooked"/> e obtido um <paramref name="reservationCode"/> único.
    /// </summary>
    public static Booking Create(Guid tripId, Guid passengerId, int seatNumber, string reservationCode, DateTime nowUtc)
    {
        if (string.IsNullOrWhiteSpace(reservationCode))
            throw new ArgumentException("O código de reserva é obrigatório.", nameof(reservationCode));

        return new Booking(tripId, passengerId, seatNumber, reservationCode, nowUtc);
    }

    /// <summary>
    /// Cancela a reserva, aplicando a regra de que o cancelamento só é permitido
    /// até <see cref="CancellationWindow"/> antes da partida.
    /// </summary>
    public void Cancel(DateTime tripDepartureUtc, DateTime nowUtc)
    {
        if (Status == BookingStatus.Cancelled)
            throw new BookingAlreadyCancelledException(ReservationCode);

        if (tripDepartureUtc - nowUtc < CancellationWindow)
            throw new CancellationWindowExpiredException(ReservationCode);

        Status = BookingStatus.Cancelled;
        CancelledAtUtc = nowUtc;
    }
}
