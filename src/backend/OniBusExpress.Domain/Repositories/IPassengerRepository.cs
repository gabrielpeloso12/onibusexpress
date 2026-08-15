using OniBusExpress.Domain.Entities;

namespace OniBusExpress.Domain.Repositories;

public interface IPassengerRepository
{
    Task<Passenger?> GetByCpfAsync(string cpfDigits, CancellationToken cancellationToken = default);
    Task AddAsync(Passenger passenger, CancellationToken cancellationToken = default);
}
