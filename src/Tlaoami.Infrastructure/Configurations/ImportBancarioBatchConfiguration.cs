using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tlaoami.Domain.Entities;

namespace Tlaoami.Infrastructure.Configurations;

public class ImportBancarioBatchConfiguration : IEntityTypeConfiguration<ImportBancarioBatch>
{
    public void Configure(EntityTypeBuilder<ImportBancarioBatch> builder)
    {
        builder.ToTable("ImportBancarioBatches");

        builder.HasKey(ib => ib.Id);

        builder.Property(ib => ib.Id).ValueGeneratedNever();
        builder.Property(ib => ib.EscuelaId).IsRequired();
        builder.Property(ib => ib.CuentaBancariaId).IsRequired();
        builder.Property(ib => ib.NombreArchivo).HasMaxLength(500).IsRequired();
        builder.Property(ib => ib.TotalMovimientos).IsRequired();
        builder.Property(ib => ib.TotalAbonos).IsRequired();
        builder.Property(ib => ib.TotalCargos).IsRequired();

        // Indices
        builder.HasIndex(ib => new { ib.EscuelaId, ib.CreatedAtUtc })
            .HasDatabaseName("IX_ImportBatches_EscuelaId_CreatedAtUtc");

        builder.HasIndex(ib => ib.CuentaBancariaId)
            .HasDatabaseName("IX_ImportBatches_CuentaBancariaId");

        // Foreign keys
        builder.HasOne(ib => ib.CuentaBancaria)
            .WithMany(cb => cb.ImportBatches)
            .HasForeignKey(ib => ib.CuentaBancariaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(ib => ib.Movimientos)
            .WithOne(mb => mb.ImportBatch)
            .HasForeignKey(mb => mb.ImportBatchId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
