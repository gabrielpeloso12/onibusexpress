using OniBusExpress.Application.DTOs;

namespace OniBusExpress.Application.Services;

public interface ITripAppService
{
    Task<IReadOnlyList<TripSummaryDto>> SearchAsync(
        string? origin,
        string? destination,
        DateOnly? date,
        CancellationToken cancellationToken = default);

    Task<TripDetailsDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
