using System.Security.Cryptography;
using OniBusExpress.Application.Abstractions;
using OniBusExpress.Domain.Repositories;

namespace OniBusExpress.Infra.Services;

/// <summary>Gera códigos no formato "ABC-12345" (3 letras maiúsculas, traço, 5 dígitos) e tenta de novo em caso de colisão.</summary>
public sealed class ReservationCodeGenerator : IReservationCodeGenerator
{
    private const string Letters = "ABCDEFGHJKLMNPQRSTUVWXYZ"; // sem I/O, para evitar confusão com 1/0
    private const int MaxAttempts = 10;

    private readonly IBookingRepository _bookingRepository;

    public ReservationCodeGenerator(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
    }

    public async Task<string> GenerateUniqueCodeAsync(CancellationToken cancellationToken = default)
    {
        for (var attempt = 0; attempt < MaxAttempts; attempt++)
        {
            var candidate = GenerateCandidate();
            if (!await _bookingRepository.ExistsByCodeAsync(candidate, cancellationToken))
                return candidate;
        }

        throw new InvalidOperationException($"Não foi possível gerar um código de reserva único após {MaxAttempts} tentativas.");
    }

    private static string GenerateCandidate()
    {
        Span<char> letters = stackalloc char[3];
        for (var i = 0; i < letters.Length; i++)
            letters[i] = Letters[RandomNumberGenerator.GetInt32(Letters.Length)];

        var digits = RandomNumberGenerator.GetInt32(0, 100_000);
        return $"{new string(letters)}-{digits:D5}";
    }
}
