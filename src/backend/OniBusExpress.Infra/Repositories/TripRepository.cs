using Microsoft.EntityFrameworkCore;
using OniBusExpress.Domain.Entities;
using OniBusExpress.Domain.Repositories;
using OniBusExpress.Infra.Persistence;

namespace OniBusExpress.Infra.Repositories;

public sealed class TripRepository : ITripRepository
{
    private readonly OniBusExpressDbContext _dbContext;

    public TripRepository(OniBusExpressDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Trip>> SearchAsync(
        string? origin,
        string? destination,
        DateOnly? date,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Trips
            .AsNoTracking()
            .Include(t => t.Route)
            .Include(t => t.Bookings)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(origin))
        {
            var originLower = origin.ToLowerInvariant();
            query = query.Where(t => t.Route!.Origin.ToLower().Contains(originLower));
        }

        if (!string.IsNullOrWhiteSpace(destination))
        {
            var destinationLower = destination.ToLowerInvariant();
            query = query.Where(t => t.Route!.Destination.ToLower().Contains(destinationLower));
        }

        if (date is not null)
        {
            var start = DateTime.SpecifyKind(date.Value.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
            var end = start.AddDays(1);
            query = query.Where(t => t.DepartureDateTime >= start && t.DepartureDateTime < end);
        }

        return await query
            .OrderBy(t => t.DepartureDateTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<Trip?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _dbContext.Trips
            .AsNoTracking()
            .Include(t => t.Route)
            .SingleOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<Trip?> GetByIdWithBookingsAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _dbContext.Trips
            .Include(t => t.Route)
            .Include(t => t.Bookings)
            .SingleOrDefaultAsync(t => t.Id == id, cancellationToken);
}
