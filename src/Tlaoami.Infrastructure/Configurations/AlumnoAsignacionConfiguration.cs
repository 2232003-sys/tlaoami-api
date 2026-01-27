using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tlaoami.Domain.Entities;

namespace Tlaoami.Infrastructure.Configurations
{
    public class AlumnoAsignacionConfiguration : IEntityTypeConfiguration<AlumnoAsignacion>
    {
        public void Configure(EntityTypeBuilder<AlumnoAsignacion> builder)
        {
            builder.ToTable("AlumnoAsignaciones");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.MontoOverride)
                   .HasPrecision(18, 2);

            builder.Property(a => a.Activo)
                   .IsRequired();

            builder.Property(a => a.CreatedAtUtc)
                   .IsRequired();

            builder.Property(a => a.UpdatedAtUtc)
                   .IsRequired(false);

            builder.HasOne(a => a.Alumno)
                   .WithMany()
                   .HasForeignKey(a => a.AlumnoId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.ConceptoCobro)
                   .WithMany()
                   .HasForeignKey(a => a.ConceptoCobroId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.CicloEscolar)
                   .WithMany()
                   .HasForeignKey(a => a.CicloId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(a => new { a.AlumnoId, a.CicloId });
            builder.HasIndex(a => new { a.AlumnoId, a.Activo });
        }
    }
}
