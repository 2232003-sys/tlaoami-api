using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tlaoami.Domain.Entities;

namespace Tlaoami.Infrastructure.Configurations;

public class MovimientoBancarioConfiguration : IEntityTypeConfiguration<MovimientoBancario>
{
    public void Configure(EntityTypeBuilder<MovimientoBancario> builder)
    {
        builder.ToTable("MovimientosBancarios");

        builder.HasKey(mb => mb.Id);

        builder.Property(mb => mb.Id).ValueGeneratedNever();
        builder.Property(mb => mb.EscuelaId).IsRequired();
        builder.Property(mb => mb.CuentaBancariaId).IsRequired();
        builder.Property(mb => mb.ImportBatchId).IsRequired(false);
        builder.Property(mb => mb.Fecha).IsRequired();
        builder.Property(mb => mb.Monto).HasPrecision(18, 2).IsRequired();
        builder.Property(mb => mb.Tipo).HasConversion<int>();
        builder.Property(mb => mb.Descripcion).HasMaxLength(500).IsRequired();
        builder.Property(mb => mb.ReferenciaBanco).HasMaxLength(200);
        builder.Property(mb => mb.Folio).HasMaxLength(100);
        builder.Property(mb => mb.HashUnico).HasMaxLength(64).IsRequired(false);
        builder.Property(mb => mb.Estado).HasConversion<int>();

        // UNIQUE constraint on HashUnico para evitar importar dos veces lo mismo
        builder.HasIndex(mb => mb.HashUnico)
            .IsUnique()
            .HasFilter("\"HashUnico\" IS NOT NULL")
            .HasDatabaseName("IX_MovimientosBancarios_HashUnico_Unique");

        // Indices para matching
        builder.HasIndex(mb => new { mb.Fecha, mb.Monto })
            .HasDatabaseName("IX_MovimientosBancarios_FechaMovimiento_Monto");

        builder.HasIndex(mb => mb.ImportBatchId)
            .HasDatabaseName("IX_MovimientosBancarios_ImportBatchId");

        builder.HasIndex(mb => new { mb.EscuelaId, mb.Estado })
            .HasDatabaseName("IX_MovimientosBancarios_EscuelaId_Estado");

        // Foreign keys
        builder.HasOne(mb => mb.CuentaBancaria)
            .WithMany(cb => cb.Movimientos)
            .HasForeignKey(mb => mb.CuentaBancariaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(mb => mb.ImportBatch)
            .WithMany(ib => ib.Movimientos)
            .HasForeignKey(mb => mb.ImportBatchId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(mb => mb.Matches)
            .WithOne(cm => cm.MovimientoBancario)
            .HasForeignKey(cm => cm.MovimientoBancarioId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
