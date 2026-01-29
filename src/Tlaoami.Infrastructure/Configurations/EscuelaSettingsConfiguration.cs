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

            // ===== INFORMACIÓN INSTITUCIONAL =====
            builder.Property(s => s.Nombre)
                   .HasMaxLength(200)
                   .IsRequired();

            builder.Property(s => s.RazonSocial)
                   .HasMaxLength(200)
                   .IsRequired();

            builder.Property(s => s.Direccion)
                   .HasMaxLength(500)
                   .IsRequired();

            builder.Property(s => s.Telefono)
                   .HasMaxLength(20)
                   .IsRequired();

            builder.Property(s => s.Email)
                   .HasMaxLength(100)
                   .IsRequired();

            builder.Property(s => s.LogoUrl)
                   .HasMaxLength(500)
                   .IsRequired();

            builder.Property(s => s.TextoRecibos)
                   .HasMaxLength(1000)
                   .IsRequired();

            // ===== CONFIGURACIÓN OPERATIVA =====
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
