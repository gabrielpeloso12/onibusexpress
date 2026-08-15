namespace OniBusExpress.Domain.Exceptions;

public sealed class TripAlreadyDepartedException : DomainException
{
    public TripAlreadyDepartedException(Guid tripId)
        : base($"A viagem '{tripId}' já foi realizada e não aceita novas reservas.")
    {
    }
}
