using OniBusExpress.Domain.Common;
using OniBusExpress.Domain.ValueObjects;

namespace OniBusExpress.Domain.Entities;

public class Passenger : Entity
{
    public string Name { get; private set; } = default!;
    public Cpf Cpf { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public DateOnly BirthDate { get; private set; }

    private Passenger()
    {
        // Construtor exigido pelo EF Core
    }

    public Passenger(string name, Cpf cpf, string email, DateOnly birthDate)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("O nome do passageiro é obrigatório.", nameof(name));

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            throw new ArgumentException("O e-mail do passageiro é inválido.", nameof(email));

        if (birthDate >= DateOnly.FromDateTime(DateTime.UtcNow))
            throw new ArgumentException("A data de nascimento deve estar no passado.", nameof(birthDate));

        Name = name.Trim();
        Cpf = cpf ?? throw new ArgumentNullException(nameof(cpf));
        Email = email.Trim();
        BirthDate = birthDate;
    }
}
