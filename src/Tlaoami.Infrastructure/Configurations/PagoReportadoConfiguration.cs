using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tlaoami.Domain.Entities;

namespace Tlaoami.Infrastructure.Configurations;

public class PagoReportadoConfiguration : IEntityTypeConfiguration<PagoReportado>
{
    public void Configure(EntityTypeBuilder<PagoReportado> builder)
    {
        builder.ToTable("PagosReportados");

        builder.HasKey(pr => pr.Id);

        builder.Property(pr => pr.Id).ValueGeneratedNever();
        builder.Property(pr => pr.EscuelaId).IsRequired();
        builder.Property(pr => pr.AlumnoId).IsRequired();
        builder.Property(pr => pr.FechaReportada).IsRequired();
        builder.Property(pr => pr.MontoReportado).HasPrecision(18, 2).IsRequired();
        builder.Property(pr => pr.MetodoPago).HasConversion<int>();
        builder.Property(pr => pr.Estatus).HasConversion<int>();
        builder.Property(pr => pr.ReferenciaTexto).HasMaxLength(500);
        builder.Property(pr => pr.ComprobanteUrl).HasMaxLength(2000);
        builder.Property(pr => pr.Notas).HasMaxLength(1000);

        // Indices para consultas comunes
        builder.HasIndex(pr => new { pr.AlumnoId, pr.Estatus })
            .HasDatabaseName("IX_PagosReportados_AlumnoId_Estatus");

        builder.HasIndex(pr => pr.FechaReportada)
            .HasDatabaseName("IX_PagosReportados_FechaReportada");

        builder.HasIndex(pr => new { pr.MontoReportado, pr.FechaReportada })
            .HasDatabaseName("IX_PagosReportados_MontoReportado_FechaReportada");

        // Foreign keys
        builder.HasOne(pr => pr.Alumno)
            .WithMany(a => a.PagosReportados)
            .HasForeignKey(pr => pr.AlumnoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
