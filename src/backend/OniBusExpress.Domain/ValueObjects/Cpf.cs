using OniBusExpress.Domain.Exceptions;

namespace OniBusExpress.Domain.ValueObjects;

/// <summary>
/// Representa um CPF (Cadastro de Pessoas Físicas) brasileiro validado.
/// Sempre armazena a forma normalizada de 11 dígitos; a validade (formato + dígitos verificadores) é
/// garantida para toda instância existente, já que a única forma de obter uma é via <see cref="Create"/>.
/// </summary>
public sealed class Cpf : IEquatable<Cpf>
{
    public const int Length = 11;

    public string Value { get; }

    private Cpf(string value)
    {
        Value = value;
    }

    public static Cpf Create(string? rawCpf)
    {
        var digits = ExtractDigits(rawCpf);

        if (!IsValid(digits))
            throw new InvalidCpfException(rawCpf ?? string.Empty);

        return new Cpf(digits);
    }

    public static bool TryCreate(string? rawCpf, out Cpf? cpf)
    {
        var digits = ExtractDigits(rawCpf);

        if (!IsValid(digits))
        {
            cpf = null;
            return false;
        }

        cpf = new Cpf(digits);
        return true;
    }

    /// <summary>
    /// Valida o formato de um CPF e seus dois dígitos verificadores, seguindo o algoritmo padrão da Receita Federal.
    /// Aceita tanto dígitos "crus" quanto uma string formatada (000.000.000-00); caracteres não numéricos são ignorados.
    /// </summary>
    public static bool IsValid(string? cpf)
    {
        var digits = ExtractDigits(cpf);
        return IsValidDigitsOnly(digits);
    }

    private static bool IsValidDigitsOnly(string digits)
    {
        if (digits.Length != Length)
            return false;

        // Sequências como "00000000000" ou "11111111111" passam na checagem de dígitos abaixo,
        // mas não são CPFs válidos — todo CPF emitido tem pelo menos dois dígitos distintos.
        if (digits.Distinct().Count() == 1)
            return false;

        var numbers = digits.Select(c => c - '0').ToArray();

        var firstCheckDigit = CalculateCheckDigit(numbers, 9);
        if (firstCheckDigit != numbers[9])
            return false;

        var secondCheckDigit = CalculateCheckDigit(numbers, 10);
        return secondCheckDigit == numbers[10];
    }

    private static int CalculateCheckDigit(int[] numbers, int length)
    {
        var sum = 0;
        var weight = length + 1;

        for (var i = 0; i < length; i++)
        {
            sum += numbers[i] * weight;
            weight--;
        }

        var remainder = sum % 11;
        return remainder < 2 ? 0 : 11 - remainder;
    }

    private static string ExtractDigits(string? value) =>
        value is null ? string.Empty : new string(value.Where(char.IsDigit).ToArray());

    public string ToFormattedString() =>
        $"{Value[..3]}.{Value[3..6]}.{Value[6..9]}-{Value[9..]}";

    public override string ToString() => Value;

    public bool Equals(Cpf? other) => other is not null && Value == other.Value;

    public override bool Equals(object? obj) => Equals(obj as Cpf);

    public override int GetHashCode() => Value.GetHashCode();
}
