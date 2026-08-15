namespace OniBusExpress.Domain.Exceptions;

public sealed class SeatAlreadyBookedException : DomainException
{
    public SeatAlreadyBookedException(Guid tripId, int seatNumber)
        : base($"O assento {seatNumber} já está ocupado para a viagem '{tripId}'.")
    {
    }
}
