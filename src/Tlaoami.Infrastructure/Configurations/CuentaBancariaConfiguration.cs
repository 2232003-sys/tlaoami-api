using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tlaoami.Domain.Entities;

namespace Tlaoami.Infrastructure.Configurations;

public class CuentaBancariaConfiguration : IEntityTypeConfiguration<CuentaBancaria>
{
    public void Configure(EntityTypeBuilder<CuentaBancaria> builder)
    {
        builder.ToTable("CuentasBancarias");

        builder.HasKey(cb => cb.Id);

        builder.Property(cb => cb.Id).ValueGeneratedNever();
        builder.Property(cb => cb.EscuelaId).IsRequired();
        builder.Property(cb => cb.Banco).HasMaxLength(100).IsRequired();
        builder.Property(cb => cb.Alias).HasMaxLength(200).IsRequired();
        builder.Property(cb => cb.Ultimos4).HasMaxLength(4).IsRequired();
        builder.Property(cb => cb.Clabe).HasMaxLength(18);
        builder.Property(cb => cb.Activa).IsRequired();

        // Indices
        builder.HasIndex(cb => new { cb.EscuelaId, cb.Activa })
            .HasDatabaseName("IX_CuentasBancarias_EscuelaId_Activa");


        // Navigation
        builder.HasMany(cb => cb.ImportBatches)
            .WithOne(ib => ib.CuentaBancaria)
            .HasForeignKey(ib => ib.CuentaBancariaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(cb => cb.Movimientos)
            .WithOne(mb => mb.CuentaBancaria)
            .HasForeignKey(mb => mb.CuentaBancariaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
