using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tlaoami.Domain.Entities;

namespace Tlaoami.Infrastructure.Configurations;

public class ConciliacionMatchConfiguration : IEntityTypeConfiguration<ConciliacionMatch>
{
    public void Configure(EntityTypeBuilder<ConciliacionMatch> builder)
    {
        builder.ToTable("ConciliacionMatches");

        builder.HasKey(cm => cm.Id);

        builder.Property(cm => cm.Id).ValueGeneratedNever();
        builder.Property(cm => cm.EscuelaId).IsRequired();
        builder.Property(cm => cm.AlumnoId).IsRequired();
        builder.Property(cm => cm.Score).IsRequired();
        builder.Property(cm => cm.ReglaMatch).HasMaxLength(100);
        builder.Property(cm => cm.Estatus).HasConversion<int>();

        // Indices para queries comunes
        builder.HasIndex(cm => new { cm.AlumnoId, cm.Estatus })
            .HasDatabaseName("IX_ConciliacionMatches_AlumnoId_Estatus");

        builder.HasIndex(cm => cm.PagoReportadoId)
            .HasDatabaseName("IX_ConciliacionMatches_PagoReportadoId");

        builder.HasIndex(cm => cm.MovimientoBancarioId)
            .HasDatabaseName("IX_ConciliacionMatches_MovimientoBancarioId");

        // UNIQUE: Un MovimientoBancario solo puede estar Confirmado una vez
        builder.HasIndex(cm => new { cm.MovimientoBancarioId, cm.Estatus })
            .IsUnique()
            .HasFilter("\"Estatus\" = 1") // Postgres filter for confirmed
            .HasDatabaseName("IX_ConciliacionMatches_MovimientoBancarioId_Confirmado_Unique");

        // Foreign keys
        builder.HasOne(cm => cm.Alumno)
            .WithMany(a => a.ConciliacionMatches)
            .HasForeignKey(cm => cm.AlumnoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cm => cm.PagoReportado)
            .WithMany(pr => pr.Matches)
            .HasForeignKey(cm => cm.PagoReportadoId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(cm => cm.MovimientoBancario)
            .WithMany(mb => mb.Matches)
            .HasForeignKey(cm => cm.MovimientoBancarioId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
