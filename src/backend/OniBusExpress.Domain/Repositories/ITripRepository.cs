using OniBusExpress.Domain.Entities;

namespace OniBusExpress.Domain.Repositories;

public interface ITripRepository
{
    /// <summary>Busca viagens por origem/destino (opcionais, sem diferenciar maiúsculas/minúsculas, correspondência parcial) e data.</summary>
    Task<IReadOnlyList<Trip>> SearchAsync(
        string? origin,
        string? destination,
        DateOnly? date,
        CancellationToken cancellationToken = default);

    Task<Trip?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Carrega uma viagem junto com suas reservas, necessário para avaliar as invariantes de ocupação de assento.</summary>
    Task<Trip?> GetByIdWithBookingsAsync(Guid id, CancellationToken cancellationToken = default);
}
