using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OniBusExpress.Domain.Entities;
using OniBusExpress.Domain.ValueObjects;

namespace OniBusExpress.Infra.Persistence.Configurations;

public sealed class PassengerConfiguration : IEntityTypeConfiguration<Passenger>
{
    public void Configure(EntityTypeBuilder<Passenger> builder)
    {
        builder.ToTable("Passengers");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Cpf)
            .IsRequired()
            .HasMaxLength(Cpf.Length)
            .HasConversion(cpf => cpf.Value, value => Cpf.Create(value))
            .HasColumnName("Cpf");

        builder.HasIndex(p => p.Cpf).IsUnique();

        builder.Property(p => p.Email)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.BirthDate)
            .IsRequired();
    }
}
