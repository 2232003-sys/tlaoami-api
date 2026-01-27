using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tlaoami.Domain.Entities;

namespace Tlaoami.Infrastructure.Configurations
{
    public class EscuelaSettingsConfiguration : IEntityTypeConfiguration<EscuelaSettings>
    {
        public void Configure(EntityTypeBuilder<EscuelaSettings> builder)
        {
            builder.ToTable("EscuelaSettings");

            builder.HasKey(s => s.Id);

            builder.HasIndex(s => s.EscuelaId)
                   .IsUnique();

            builder.Property(s => s.DiaCorteColegiatura)
                   .IsRequired();

            builder.Property(s => s.BloquearReinscripcionConSaldo)
                   .IsRequired();

            builder.Property(s => s.ZonaHoraria)
                   .HasMaxLength(100)
                   .IsRequired();

            builder.Property(s => s.Moneda)
                   .HasMaxLength(10)
                   .IsRequired();

            builder.Property(s => s.CreatedAtUtc)
                   .IsRequired();

            builder.Property(s => s.UpdatedAtUtc)
                   .IsRequired(false);
        }
    }
}
