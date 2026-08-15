using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DomainRoute = OniBusExpress.Domain.Entities.Route;

namespace OniBusExpress.Infra.Persistence.Configurations;

public sealed class RouteConfiguration : IEntityTypeConfiguration<DomainRoute>
{
    public void Configure(EntityTypeBuilder<DomainRoute> builder)
    {
        builder.ToTable("Routes");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Origin)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(r => r.Destination)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(r => r.EstimatedDuration)
            .IsRequired();

        builder.HasMany(r => r.Trips)
            .WithOne(t => t.Route)
            .HasForeignKey(t => t.RouteId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
