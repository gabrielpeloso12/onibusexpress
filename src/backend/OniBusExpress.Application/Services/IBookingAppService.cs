using OniBusExpress.Application.DTOs;

namespace OniBusExpress.Application.Services;

public interface IBookingAppService
{
    Task<BookingResponseDto> CreateAsync(CreateBookingRequest request, CancellationToken cancellationToken = default);

    Task<BookingResponseDto?> GetByCodeAsync(string reservationCode, CancellationToken cancellationToken = default);

    /// <summary>Retorna <c>false</c> quando não existe reserva com o código informado.</summary>
    Task<bool> CancelAsync(string reservationCode, CancellationToken cancellationToken = default);
}
