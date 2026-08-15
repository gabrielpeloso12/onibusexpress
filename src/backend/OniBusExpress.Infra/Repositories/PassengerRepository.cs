using Microsoft.EntityFrameworkCore;
using OniBusExpress.Domain.Entities;
using OniBusExpress.Domain.Repositories;
using OniBusExpress.Domain.ValueObjects;
using OniBusExpress.Infra.Persistence;

namespace OniBusExpress.Infra.Repositories;

public sealed class PassengerRepository : IPassengerRepository
{
    private readonly OniBusExpressDbContext _dbContext;

    public PassengerRepository(OniBusExpressDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Passenger?> GetByCpfAsync(string cpfDigits, CancellationToken cancellationToken = default)
    {
        var cpf = Cpf.Create(cpfDigits);
        return await _dbContext.Passengers.SingleOrDefaultAsync(p => p.Cpf == cpf, cancellationToken);
    }

    public async Task AddAsync(Passenger passenger, CancellationToken cancellationToken = default) =>
        await _dbContext.Passengers.AddAsync(passenger, cancellationToken);
}
