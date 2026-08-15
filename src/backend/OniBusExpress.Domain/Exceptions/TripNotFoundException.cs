namespace OniBusExpress.Domain.Exceptions;

public sealed class TripNotFoundException : DomainException
{
    public TripNotFoundException(Guid tripId)
        : base($"Nenhuma viagem foi encontrada com o identificador '{tripId}'.")
    {
    }
}
