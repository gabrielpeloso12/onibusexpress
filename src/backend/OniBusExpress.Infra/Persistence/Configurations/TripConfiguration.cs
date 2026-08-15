using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OniBusExpress.Domain.Entities;

namespace OniBusExpress.Infra.Persistence.Configurations;

public sealed class TripConfiguration : IEntityTypeConfiguration<Trip>
{
    public void Configure(EntityTypeBuilder<Trip> builder)
    {
        builder.ToTable("Trips");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.DepartureDateTime)
            .IsRequired();

        builder.Property(t => t.BasePrice)
            .IsRequired()
            .HasColumnType("decimal(10,2)");

        builder.Property(t => t.TotalSeats)
            .IsRequired();

        builder.HasMany(t => t.Bookings)
            .WithOne(b => b.Trip)
            .HasForeignKey(b => b.TripId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(t => new { t.RouteId, t.DepartureDateTime });
    }
}
