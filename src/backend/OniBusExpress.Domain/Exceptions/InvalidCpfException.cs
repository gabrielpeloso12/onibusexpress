namespace OniBusExpress.Domain.Exceptions;

public sealed class InvalidCpfException : DomainException
{
    public InvalidCpfException(string cpf)
        : base($"O CPF '{cpf}' é inválido.")
    {
    }
}
