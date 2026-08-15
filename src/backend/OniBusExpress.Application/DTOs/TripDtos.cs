namespace OniBusExpress.Application.DTOs;

/// <summary>Item do payload de resposta de <c>GET /viagens</c>.</summary>
public sealed record TripSummaryDto(
    Guid Id,
    Guid RouteId,
    string Origin,
    string Destination,
    DateTime DepartureDateTime,
    decimal BasePrice,
    int AvailableSeats,
    int TotalSeats);

/// <summary>Payload de resposta de <c>GET /viagens/{id}</c>, incluindo a ocupação de cada assento.</summary>
public sealed record TripDetailsDto(
    Guid Id,
    Guid RouteId,
    string Origin,
    string Destination,
    TimeSpan EstimatedDuration,
    DateTime DepartureDateTime,
    decimal BasePrice,
    int TotalSeats,
    IReadOnlyList<SeatDto> Seats);

public sealed record SeatDto(int SeatNumber, bool IsOccupied);
