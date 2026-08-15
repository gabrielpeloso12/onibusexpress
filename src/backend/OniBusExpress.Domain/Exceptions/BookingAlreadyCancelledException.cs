namespace OniBusExpress.Domain.Exceptions;

public sealed class BookingAlreadyCancelledException : DomainException
{
    public BookingAlreadyCancelledException(string reservationCode)
        : base($"A reserva '{reservationCode}' já está cancelada.")
    {
    }
}
