using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tlaoami.Domain.Entities;

namespace Tlaoami.Infrastructure.Configurations
{
    public class OrdenVentaConfiguration : IEntityTypeConfiguration<OrdenVenta>
    {
        public void Configure(EntityTypeBuilder<OrdenVenta> builder)
        {
            builder.ToTable("OrdenesVenta");

            builder.HasKey(o => o.Id);

            builder.Property(o => o.Estatus)
                   .HasConversion<string>()
                   .IsRequired();

            builder.Property(o => o.Total)
                   .HasPrecision(18, 2)
                   .IsRequired();

            builder.Property(o => o.Notas)
                   .HasMaxLength(1000);

            builder.Property(o => o.Fecha)
                   .IsRequired();

            builder.Property(o => o.CreatedAtUtc)
                   .IsRequired();

            builder.HasOne(o => o.Alumno)
                   .WithMany()
                   .HasForeignKey(o => o.AlumnoId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(o => o.Lineas)
                   .WithOne(l => l.OrdenVenta)
                   .HasForeignKey(l => l.OrdenVentaId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(o => o.AlumnoId);
            builder.HasIndex(o => o.Fecha);
            builder.HasIndex(o => o.Estatus);
        }
    }
}
