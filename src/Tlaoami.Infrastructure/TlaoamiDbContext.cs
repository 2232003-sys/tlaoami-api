using Microsoft.EntityFrameworkCore;
using Tlaoami.Domain.Entities;

public class TlaoamiDbContext : DbContext
{
    public TlaoamiDbContext(DbContextOptions<TlaoamiDbContext> options) : base(options)
    {
    }

    public DbSet<Alumno> Alumnos { get; set; }
    public DbSet<CicloEscolar> CiclosEscolares { get; set; }
    public DbSet<Grupo> Grupos { get; set; }
    public DbSet<AlumnoGrupo> AsignacionesGrupo { get; set; }
    public DbSet<Factura> Facturas { get; set; }
    public DbSet<Pago> Pagos { get; set; }
    public DbSet<PaymentIntent> PaymentIntents { get; set; }
    public DbSet<MovimientoBancario> MovimientosBancarios { get; set; }
    public DbSet<MovimientoConciliacion> MovimientosConciliacion { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Alumno configuration
        modelBuilder.Entity<Alumno>()
            .HasIndex(a => a.Matricula)
            .IsUnique();

        modelBuilder.Entity<Alumno>()
            .HasIndex(a => a.Email)
            .IsUnique();

        // CicloEscolar configuration
        modelBuilder.Entity<CicloEscolar>()
            .HasMany(c => c.Grupos)
            .WithOne(g => g.CicloEscolar)
            .HasForeignKey(g => g.CicloEscolarId)
            .OnDelete(DeleteBehavior.Cascade);

        // Grupo configuration
        modelBuilder.Entity<Grupo>()
            .HasMany(g => g.Alumnos)
            .WithOne(ag => ag.Grupo)
            .HasForeignKey(ag => ag.GrupoId)
            .OnDelete(DeleteBehavior.Cascade);

        // AlumnoGrupo configuration
        modelBuilder.Entity<AlumnoGrupo>()
            .HasOne(ag => ag.Alumno)
            .WithMany(a => a.AsignacionesGrupo)
            .HasForeignKey(ag => ag.AlumnoId)
            .OnDelete(DeleteBehavior.Cascade);

        // Index para consultas de asignaciones activas
        modelBuilder.Entity<AlumnoGrupo>()
            .HasIndex(ag => new { ag.AlumnoId, ag.Activo })
            .HasDatabaseName("IX_AlumnoGrupo_AlumnoId_Activo");

        modelBuilder.Entity<AlumnoGrupo>()
            .HasIndex(ag => new { ag.GrupoId, ag.Activo })
            .HasDatabaseName("IX_AlumnoGrupo_GrupoId_Activo");

        // Factura configuration
        modelBuilder.Entity<Factura>()
            .HasIndex(f => f.NumeroFactura)
            .IsUnique();

        modelBuilder.Entity<Factura>()
            .Property(f => f.Estado)
            .HasConversion<string>();

        modelBuilder.Entity<Factura>()
            .HasOne(f => f.Alumno)
            .WithMany(a => a.Facturas) // Explicitly configure the relationship
            .HasForeignKey(f => f.AlumnoId);

        // Pago configuration
        modelBuilder.Entity<Pago>()
            .Property(p => p.Metodo)
            .HasConversion<string>();

        modelBuilder.Entity<Pago>()
            .Property(p => p.IdempotencyKey)
            .HasMaxLength(128);

        modelBuilder.Entity<Pago>()
            .HasOne(p => p.Factura)
            .WithMany(f => f.Pagos)
            .HasForeignKey(p => p.FacturaId);

        modelBuilder.Entity<Pago>()
            .HasIndex(p => new { p.FacturaId, p.IdempotencyKey })
            .IsUnique();

        modelBuilder.Entity<Pago>()
            .HasIndex(p => p.PaymentIntentId)
            .IsUnique()
            .HasFilter("\"PaymentIntentId\" IS NOT NULL");

        // PaymentIntent configuration
        modelBuilder.Entity<PaymentIntent>()
            .Property(pi => pi.Metodo)
            .HasConversion<string>();

        modelBuilder.Entity<PaymentIntent>()
            .Property(pi => pi.Estado)
            .HasConversion<string>();

        modelBuilder.Entity<PaymentIntent>()
            .HasOne<Factura>()
            .WithMany()
            .HasForeignKey(pi => pi.FacturaId);

        // MovimientoBancario configuration
        modelBuilder.Entity<MovimientoBancario>()
            .Property(mb => mb.Tipo)
            .HasConversion<string>();

        modelBuilder.Entity<MovimientoBancario>()
            .Property(mb => mb.Estado)
            .HasConversion<string>();

        modelBuilder.Entity<MovimientoBancario>()
            .HasIndex(mb => mb.HashMovimiento)
            .IsUnique();

        // MovimientoConciliacion configuration
        modelBuilder.Entity<MovimientoConciliacion>()
            .HasOne(mc => mc.MovimientoBancario)
            .WithMany()
            .HasForeignKey(mc => mc.MovimientoBancarioId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MovimientoConciliacion>()
            .HasOne(mc => mc.Alumno)
            .WithMany()
            .HasForeignKey(mc => mc.AlumnoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MovimientoConciliacion>()
            .HasOne(mc => mc.Factura)
            .WithMany()
            .HasForeignKey(mc => mc.FacturaId)
            .OnDelete(DeleteBehavior.Restrict);

        // User configuration
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();
    }
}
