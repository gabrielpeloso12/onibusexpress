namespace OniBusExpress.Application.Abstractions;

/// <summary>Gera um código de reserva único e legível (ex.: "ABC-12345").</summary>
public interface IReservationCodeGenerator
{
    Task<string> GenerateUniqueCodeAsync(CancellationToken cancellationToken = default);
}
