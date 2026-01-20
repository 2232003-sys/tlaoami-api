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
    public DbSet<ConceptoCobro> ConceptosCobro { get; set; }
    public DbSet<ReglaCobroPorCiclo> ReglasCobro { get; set; }
    public DbSet<AvisoPrivacidad> AvisosPrivacidad { get; set; }
    public DbSet<AceptacionAvisoPrivacidad> AceptacionesAvisoPrivacidad { get; set; }
    public DbSet<Reinscripcion> Reinscripciones { get; set; }

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

        // ConceptoCobro configuration
        modelBuilder.Entity<ConceptoCobro>()
            .HasIndex(c => c.Clave)
            .IsUnique();

        modelBuilder.Entity<ConceptoCobro>()
            .Property(c => c.Clave)
            .HasMaxLength(30)
            .IsRequired();

        modelBuilder.Entity<ConceptoCobro>()
            .Property(c => c.Nombre)
            .HasMaxLength(120)
            .IsRequired();

        modelBuilder.Entity<ConceptoCobro>()
            .Property(c => c.Periodicidad)
            .HasConversion<string>(); // Enum stored as string

        // ReglaCobroPorCiclo configuration
        modelBuilder.Entity<ReglaCobroPorCiclo>()
            .HasOne(r => r.CicloEscolar)
            .WithMany()
            .HasForeignKey(r => r.CicloId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ReglaCobroPorCiclo>()
            .HasOne(r => r.ConceptoCobro)
            .WithMany()
            .HasForeignKey(r => r.ConceptoCobroId)
            .OnDelete(DeleteBehavior.Restrict);

        // Unique logical index: (CicloId, Grado, Turno, ConceptoCobroId, TipoGeneracion)
        modelBuilder.Entity<ReglaCobroPorCiclo>()
            .HasIndex(r => new { r.CicloId, r.Grado, r.Turno, r.ConceptoCobroId, r.TipoGeneracion })
            .IsUnique()
            .HasDatabaseName("IX_ReglasCobro_Unique_Logico");

        modelBuilder.Entity<ReglaCobroPorCiclo>()
            .Property(r => r.TipoGeneracion)
            .HasConversion<string>(); // Enum stored as string

        modelBuilder.Entity<ReglaCobroPorCiclo>()
            .Property(r => r.Turno)
            .HasMaxLength(50);

        modelBuilder.Entity<ReglaCobroPorCiclo>()
            .Property(r => r.MontoBase)
            .HasPrecision(18, 2);

        // === AvisoPrivacidad Configuration ===
        modelBuilder.Entity<AvisoPrivacidad>()
            .HasKey(a => a.Id);

        modelBuilder.Entity<AvisoPrivacidad>()
            .Property(a => a.Version)
            .IsRequired()
            .HasMaxLength(50);

        modelBuilder.Entity<AvisoPrivacidad>()
            .Property(a => a.Contenido)
            .IsRequired();

        modelBuilder.Entity<AvisoPrivacidad>()
            .HasIndex(a => a.Vigente)
            .IsUnique()
            .HasFilter($"Vigente = true"); // Índice único solo en registros vigentes

        modelBuilder.Entity<AvisoPrivacidad>()
            .HasMany(a => a.Aceptaciones)
            .WithOne(aa => aa.AvisoPrivacidad)
            .HasForeignKey(aa => aa.AvisoPrivacidadId)
            .OnDelete(DeleteBehavior.Cascade);

        // === AceptacionAvisoPrivacidad Configuration ===
        modelBuilder.Entity<AceptacionAvisoPrivacidad>()
            .HasKey(aa => aa.Id);

        modelBuilder.Entity<AceptacionAvisoPrivacidad>()
            .HasIndex(aa => new { aa.UsuarioId, aa.AvisoPrivacidadId })
            .IsUnique(); // Garantiza que cada usuario acepta solo una vez cada aviso

        modelBuilder.Entity<AceptacionAvisoPrivacidad>()
            .Property(aa => aa.Ip)
            .HasMaxLength(45); // IPv6 puede ser hasta 45 caracteres

        modelBuilder.Entity<AceptacionAvisoPrivacidad>()
            .Property(aa => aa.UserAgent)
            .HasMaxLength(500);

        modelBuilder.Entity<AceptacionAvisoPrivacidad>()
            .HasOne(aa => aa.Usuario)
            .WithMany()
            .HasForeignKey(aa => aa.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        // === Reinscripcion Configuration ===
        modelBuilder.Entity<Reinscripcion>()
            .HasKey(r => r.Id);

        modelBuilder.Entity<Reinscripcion>()
            .HasIndex(r => new { r.AlumnoId, r.CicloDestinoId })
            .IsUnique(); // Un alumno una reinscripción por ciclo

        modelBuilder.Entity<Reinscripcion>()
            .Property(r => r.Estado)
            .HasMaxLength(50);

        modelBuilder.Entity<Reinscripcion>()
            .Property(r => r.MotivoBloqueo)
            .HasMaxLength(100);

        modelBuilder.Entity<Reinscripcion>()
            .Property(r => r.SaldoAlMomento)
            .HasPrecision(18, 2);

        // Foreign Keys
        modelBuilder.Entity<Reinscripcion>()
            .HasOne(r => r.Alumno)
            .WithMany()
            .HasForeignKey(r => r.AlumnoId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Reinscripcion>()
            .HasOne(r => r.CicloOrigen)
            .WithMany()
            .HasForeignKey(r => r.CicloOrigenId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Reinscripcion>()
            .HasOne(r => r.GrupoOrigen)
            .WithMany()
            .HasForeignKey(r => r.GrupoOrigenId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Reinscripcion>()
            .HasOne(r => r.CicloDestino)
            .WithMany()
            .HasForeignKey(r => r.CicloDestinoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Reinscripcion>()
            .HasOne(r => r.GrupoDestino)
            .WithMany()
            .HasForeignKey(r => r.GrupoDestinoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
