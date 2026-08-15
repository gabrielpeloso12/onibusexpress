using OniBusExpress.Domain.Repositories;
using OniBusExpress.Infra.Persistence;

namespace OniBusExpress.Infra.Repositories;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly OniBusExpressDbContext _dbContext;

    public UnitOfWork(OniBusExpressDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _dbContext.SaveChangesAsync(cancellationToken);
}
