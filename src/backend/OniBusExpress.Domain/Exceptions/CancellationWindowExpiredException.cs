namespace OniBusExpress.Domain.Exceptions;

public sealed class CancellationWindowExpiredException : DomainException
{
    public CancellationWindowExpiredException(string reservationCode)
        : base($"A reserva '{reservationCode}' não pode mais ser cancelada: faltam menos de 2 horas para a partida.")
    {
    }
}
