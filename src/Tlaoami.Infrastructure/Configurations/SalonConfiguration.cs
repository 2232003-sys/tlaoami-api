using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tlaoami.Domain.Entities;

namespace Tlaoami.Infrastructure.Configurations;

public class SalonConfiguration : IEntityTypeConfiguration<Salon>
{
    public void Configure(EntityTypeBuilder<Salon> builder)
    {
        builder.ToTable("Salones");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Codigo)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Nombre)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(x => x.Activo)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasIndex(x => x.Codigo)
            .IsUnique();
    }
}
