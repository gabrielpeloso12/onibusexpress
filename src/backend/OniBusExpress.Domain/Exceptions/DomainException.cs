namespace OniBusExpress.Domain.Exceptions;

/// <summary>
/// Tipo base para toda violação de regra de negócio lançada pela camada de domínio.
/// É capturada pelo middleware de tratamento de exceções da API e traduzida em uma resposta HTTP.
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message)
    {
    }
}
