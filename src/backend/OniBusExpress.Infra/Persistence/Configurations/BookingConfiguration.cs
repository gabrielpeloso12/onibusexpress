using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OniBusExpress.Domain.Entities;

namespace OniBusExpress.Infra.Persistence.Configurations;

public sealed class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("Bookings");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.SeatNumber)
            .IsRequired();

        builder.Property(b => b.ReservationCode)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(b => b.ReservationCode).IsUnique();

        builder.Property(b => b.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(b => b.CreatedAtUtc)
            .IsRequired();

        builder.HasOne(b => b.Passenger)
            .WithMany()
            .HasForeignKey(b => b.PassengerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Impede que duas reservas ativas ocupem o mesmo assento na mesma viagem,
        // como uma trava a nível de banco para a invariante de ocupação de assento do domínio.
        builder.HasIndex(b => new { b.TripId, b.SeatNumber })
            .IsUnique()
            .HasFilter("\"Status\" = 'Confirmed'");
    }
}
