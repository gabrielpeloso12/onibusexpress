using OniBusExpress.Domain.Entities;

namespace OniBusExpress.Domain.Repositories;

public interface IBookingRepository
{
    Task AddAsync(Booking booking, CancellationToken cancellationToken = default);

    /// <summary>Carrega uma reserva pelo código, junto com a viagem e o passageiro associados.</summary>
    Task<Booking?> GetByCodeAsync(string reservationCode, CancellationToken cancellationToken = default);

    Task<bool> ExistsByCodeAsync(string reservationCode, CancellationToken cancellationToken = default);
}
