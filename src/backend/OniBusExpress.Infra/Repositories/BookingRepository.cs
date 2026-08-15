using Microsoft.EntityFrameworkCore;
using OniBusExpress.Domain.Entities;
using OniBusExpress.Domain.Repositories;
using OniBusExpress.Infra.Persistence;

namespace OniBusExpress.Infra.Repositories;

public sealed class BookingRepository : IBookingRepository
{
    private readonly OniBusExpressDbContext _dbContext;

    public BookingRepository(OniBusExpressDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Booking booking, CancellationToken cancellationToken = default) =>
        await _dbContext.Bookings.AddAsync(booking, cancellationToken);

    public async Task<Booking?> GetByCodeAsync(string reservationCode, CancellationToken cancellationToken = default) =>
        await _dbContext.Bookings
            .Include(b => b.Trip)
                .ThenInclude(t => t!.Route)
            .Include(b => b.Passenger)
            .SingleOrDefaultAsync(b => b.ReservationCode == reservationCode, cancellationToken);

    public async Task<bool> ExistsByCodeAsync(string reservationCode, CancellationToken cancellationToken = default) =>
        await _dbContext.Bookings.AnyAsync(b => b.ReservationCode == reservationCode, cancellationToken);
}
