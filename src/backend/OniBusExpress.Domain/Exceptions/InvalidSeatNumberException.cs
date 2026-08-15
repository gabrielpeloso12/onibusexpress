namespace OniBusExpress.Domain.Exceptions;

public sealed class InvalidSeatNumberException : DomainException
{
    public InvalidSeatNumberException(int seatNumber, int totalSeats)
        : base($"O assento {seatNumber} é inválido. A viagem possui assentos numerados de 1 a {totalSeats}.")
    {
    }
}
