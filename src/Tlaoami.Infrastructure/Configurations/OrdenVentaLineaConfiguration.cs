using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tlaoami.Domain.Entities;

namespace Tlaoami.Infrastructure.Configurations
{
    public class OrdenVentaLineaConfiguration : IEntityTypeConfiguration<OrdenVentaLinea>
    {
        public void Configure(EntityTypeBuilder<OrdenVentaLinea> builder)
        {
            builder.ToTable("OrdenesVentaLineas");

            builder.HasKey(l => l.Id);

            builder.Property(l => l.Cantidad)
                   .IsRequired();

            builder.Property(l => l.PrecioUnitario)
                   .HasPrecision(18, 2)
                   .IsRequired();

            builder.Property(l => l.Subtotal)
                   .HasPrecision(18, 2)
                   .IsRequired();

            builder.Property(l => l.CreatedAtUtc)
                   .IsRequired();

            builder.HasOne(l => l.Producto)
                   .WithMany()
                   .HasForeignKey(l => l.ProductoId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(l => l.OrdenVentaId);
            builder.HasIndex(l => l.ProductoId);
        }
    }
}
