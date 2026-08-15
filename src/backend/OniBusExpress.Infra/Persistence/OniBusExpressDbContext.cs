using Microsoft.EntityFrameworkCore;
using OniBusExpress.Domain.Entities;
using DomainRoute = OniBusExpress.Domain.Entities.Route;

namespace OniBusExpress.Infra.Persistence;

public sealed class OniBusExpressDbContext : DbContext
{
    public OniBusExpressDbContext(DbContextOptions<OniBusExpressDbContext> options) : base(options)
    {
    }

    public DbSet<DomainRoute> Routes => Set<DomainRoute>();
    public DbSet<Trip> Trips => Set<Trip>();
    public DbSet<Passenger> Passengers => Set<Passenger>();
    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OniBusExpressDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
