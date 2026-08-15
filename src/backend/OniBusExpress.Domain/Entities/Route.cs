using OniBusExpress.Domain.Common;

namespace OniBusExpress.Domain.Entities;

/// <summary>Uma rota operada pela empresa, ex.: São Paulo → Rio de Janeiro.</summary>
public class Route : Entity
{
    public string Origin { get; private set; } = default!;
    public string Destination { get; private set; } = default!;
    public TimeSpan EstimatedDuration { get; private set; }

    private readonly List<Trip> _trips = new();
    public IReadOnlyCollection<Trip> Trips => _trips.AsReadOnly();

    private Route()
    {
        // Construtor exigido pelo EF Core
    }

    public Route(string origin, string destination, TimeSpan estimatedDuration)
    {
        if (string.IsNullOrWhiteSpace(origin))
            throw new ArgumentException("A origem da rota é obrigatória.", nameof(origin));

        if (string.IsNullOrWhiteSpace(destination))
            throw new ArgumentException("O destino da rota é obrigatório.", nameof(destination));

        if (string.Equals(origin, destination, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("A origem e o destino da rota não podem ser iguais.");

        if (estimatedDuration <= TimeSpan.Zero)
            throw new ArgumentException("A duração estimada deve ser maior que zero.", nameof(estimatedDuration));

        Origin = origin.Trim();
        Destination = destination.Trim();
        EstimatedDuration = estimatedDuration;
    }
}
