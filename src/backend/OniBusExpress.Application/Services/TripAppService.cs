using OniBusExpress.Application.DTOs;
using OniBusExpress.Domain.Repositories;

namespace OniBusExpress.Application.Services;

public sealed class TripAppService : ITripAppService
{
    private readonly ITripRepository _tripRepository;

    public TripAppService(ITripRepository tripRepository)
    {
        _tripRepository = tripRepository;
    }

    public async Task<IReadOnlyList<TripSummaryDto>> SearchAsync(
        string? origin,
        string? destination,
        DateOnly? date,
        CancellationToken cancellationToken = default)
    {
        var trips = await _tripRepository.SearchAsync(origin, destination, date, cancellationToken);

        return trips
            .Select(t => new TripSummaryDto(
                t.Id,
                t.RouteId,
                t.Route!.Origin,
                t.Route!.Destination,
                t.DepartureDateTime,
                t.BasePrice,
                t.GetAvailableSeatNumbers().Count(),
                t.TotalSeats))
            .ToList();
    }

    public async Task<TripDetailsDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var trip = await _tripRepository.GetByIdWithBookingsAsync(id, cancellationToken);
        if (trip is null)
            return null;

        var occupied = trip.GetOccupiedSeatNumbers().ToHashSet();
        var seats = Enumerable.Range(1, trip.TotalSeats)
            .Select(seatNumber => new SeatDto(seatNumber, occupied.Contains(seatNumber)))
            .ToList();

        return new TripDetailsDto(
            trip.Id,
            trip.RouteId,
            trip.Route!.Origin,
            trip.Route!.Destination,
            trip.Route!.EstimatedDuration,
            trip.DepartureDateTime,
            trip.BasePrice,
            trip.TotalSeats,
            seats);
    }
}
