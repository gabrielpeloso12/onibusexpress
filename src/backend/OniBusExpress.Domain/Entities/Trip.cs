using OniBusExpress.Domain.Common;
using OniBusExpress.Domain.Enums;
using OniBusExpress.Domain.Exceptions;

namespace OniBusExpress.Domain.Entities;

/// <summary>
/// Uma partida agendada de uma <see cref="Route"/>. É dona das invariantes de reserva de assento:
/// não permite reservar o mesmo assento duas vezes nem reservar uma viagem que já partiu.
/// </summary>
public class Trip : Entity
{
    public Guid RouteId { get; private set; }
    public Route? Route { get; private set; }
    public DateTime DepartureDateTime { get; private set; }
    public decimal BasePrice { get; private set; }
    public int TotalSeats { get; private set; }

    private readonly List<Booking> _bookings = new();
    public IReadOnlyCollection<Booking> Bookings => _bookings.AsReadOnly();

    private Trip()
    {
        // Construtor exigido pelo EF Core
    }

    public Trip(Guid routeId, DateTime departureDateTime, decimal basePrice, int totalSeats)
    {
        if (routeId == Guid.Empty)
            throw new ArgumentException("A rota da viagem é obrigatória.", nameof(routeId));

        if (basePrice <= 0)
            throw new ArgumentException("O preço base deve ser maior que zero.", nameof(basePrice));

        if (totalSeats <= 0)
            throw new ArgumentException("A viagem deve ter ao menos um assento.", nameof(totalSeats));

        RouteId = routeId;
        DepartureDateTime = departureDateTime;
        BasePrice = basePrice;
        TotalSeats = totalSeats;
    }

    /// <summary>Anexa uma reserva já persistida, usado em testes para simular um agregado carregado.</summary>
    internal void AttachExistingBooking(Booking booking) => _bookings.Add(booking);

    public bool HasDeparted(DateTime now) => DepartureDateTime <= now;

    public bool IsSeatOccupied(int seatNumber) =>
        _bookings.Any(b => b.SeatNumber == seatNumber && b.Status == BookingStatus.Confirmed);

    public IEnumerable<int> GetOccupiedSeatNumbers() =>
        _bookings.Where(b => b.Status == BookingStatus.Confirmed)
                 .Select(b => b.SeatNumber)
                 .OrderBy(n => n);

    public IEnumerable<int> GetAvailableSeatNumbers() =>
        Enumerable.Range(1, TotalSeats).Except(GetOccupiedSeatNumbers());

    /// <summary>
    /// Lança uma exceção de domínio se esta viagem não puder aceitar uma reserva para o assento informado.
    /// Deve ser chamado com a coleção <see cref="Bookings"/> carregada (via include) para que a
    /// checagem de ocupação seja precisa.
    /// </summary>
    public void EnsureCanBeBooked(int seatNumber, DateTime now)
    {
        if (seatNumber < 1 || seatNumber > TotalSeats)
            throw new InvalidSeatNumberException(seatNumber, TotalSeats);

        if (HasDeparted(now))
            throw new TripAlreadyDepartedException(Id);

        if (IsSeatOccupied(seatNumber))
            throw new SeatAlreadyBookedException(Id, seatNumber);
    }
}
