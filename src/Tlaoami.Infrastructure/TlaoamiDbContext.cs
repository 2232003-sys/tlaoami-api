using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Tlaoami.Domain.Entities;
using Tlaoami.Infrastructure.Configurations;

public class TlaoamiDbContext : DbContext
{
    public TlaoamiDbContext(DbContextOptions<TlaoamiDbContext> options) : base(options)
    {
    }

    public DbSet<Alumno> Alumnos { get; set; }
    public DbSet<CicloEscolar> CiclosEscolares { get; set; }
    public DbSet<Grupo> Grupos { get; set; }
    public DbSet<AlumnoGrupo> AsignacionesGrupo { get; set; }
    public DbSet<AlumnoAsignacion> AlumnoAsignaciones { get; set; }
    public DbSet<OrdenVenta> OrdenesVenta { get; set; }
    public DbSet<OrdenVentaLinea> OrdenesVentaLineas { get; set; }
    public DbSet<Factura> Facturas { get; set; }
    public DbSet<FacturaLinea> FacturaLineas { get; set; }
    public DbSet<Pago> Pagos { get; set; }
    public DbSet<PaymentIntent> PaymentIntents { get; set; }
    public DbSet<MovimientoBancario> MovimientosBancarios { get; set; }
    public DbSet<MovimientoConciliacion> MovimientosConciliacion { get; set; }
    public DbSet<CuentaBancaria> CuentasBancarias { get; set; }
    public DbSet<PagoReportado> PagosReportados { get; set; }
    public DbSet<ImportBancarioBatch> ImportBatches { get; set; }
    public DbSet<ConciliacionMatch> ConciliacionMatches { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<ConceptoCobro> ConceptosCobro { get; set; }
    public DbSet<ReglaCobroPorCiclo> ReglasCobro { get; set; }
    public DbSet<ReglaColegiatura> ReglasColegiatura { get; set; }
    public DbSet<BecaAlumno> BecasAlumno { get; set; }
    public DbSet<ReglaRecargo> ReglasRecargo { get; set; }
    public DbSet<AvisoPrivacidad> AvisosPrivacidad { get; set; }
    public DbSet<AceptacionAvisoPrivacidad> AceptacionesAvisoPrivacidad { get; set; }
    public DbSet<Reinscripcion> Reinscripciones { get; set; }
    public DbSet<Salon> Salones => Set<Salon>();
    public DbSet<ReceptorFiscal> ReceptoresFiscales { get; set; }
    public DbSet<FacturaFiscal> FacturasFiscales { get; set; }
    public DbSet<EscuelaSettings> EscuelaSettings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply entity configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TlaoamiDbContext).Assembly);

        // === GLOBAL DateTime to UTC Conversion ===
        // Configure all DateTime properties to convert to UTC for PostgreSQL
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                {
                    property.SetValueConverter(new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>(
                        v => v.Kind == DateTimeKind.Utc ? v : v.ToUniversalTime(),
                        v => DateTime.SpecifyKind(v, DateTimeKind.Utc)
                    ));

                    if (property.ClrType == typeof(DateTime?))
                    {
                        property.SetValueConverter(new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime?, DateTime?>(
                            v => v.HasValue ? (v.Value.Kind == DateTimeKind.Utc ? v : v.Value.ToUniversalTime()) : null,
                            v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null
                        ));
                    }
                }
            }
        }

        // Alumno configuration
        modelBuilder.Entity<Alumno>()
            .HasIndex(a => a.Matricula)
            .IsUnique();

        // EscuelaSettings configuration (unique per EscuelaId)
        modelBuilder.Entity<EscuelaSettings>()
            .HasIndex(s => s.EscuelaId)
            .IsUnique();

        modelBuilder.Entity<EscuelaSettings>()
            .Property(s => s.ZonaHoraria)
            .HasMaxLength(100)
            .IsRequired();

        modelBuilder.Entity<EscuelaSettings>()
            .Property(s => s.Moneda)
            .HasMaxLength(10)
            .IsRequired();

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
            .Property(f => f.TipoDocumento)
            .HasConversion<string>();

        modelBuilder.Entity<Factura>()
            .Property(f => f.Periodo)
            .HasMaxLength(7);

        modelBuilder.Entity<Factura>()
            .HasOne(f => f.Alumno)
            .WithMany(a => a.Facturas) // Explicitly configure the relationship
            .HasForeignKey(f => f.AlumnoId);

        modelBuilder.Entity<Factura>()
            .HasOne(f => f.ConceptoCobro)
            .WithMany()
            .HasForeignKey(f => f.ConceptoCobroId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Factura>()
            .HasIndex(f => new { f.AlumnoId, f.Periodo, f.ConceptoCobroId })
            .IsUnique()
            .HasFilter("\"Estado\" <> 'Cancelada'")
            .HasDatabaseName("IX_Factura_Alumno_Periodo_Concepto");

        // FacturaLinea configuration
        modelBuilder.Entity<FacturaLinea>()
            .Property(fl => fl.Descripcion)
            .HasMaxLength(500)
            .IsRequired();

        modelBuilder.Entity<FacturaLinea>()
            .Property(fl => fl.Subtotal)
            .HasPrecision(18, 2);

        modelBuilder.Entity<FacturaLinea>()
            .Property(fl => fl.Descuento)
            .HasPrecision(18, 2);

        modelBuilder.Entity<FacturaLinea>()
            .Property(fl => fl.Impuesto)
            .HasPrecision(18, 2);

        modelBuilder.Entity<FacturaLinea>()
            .HasOne(fl => fl.Factura)
            .WithMany(f => f.Lineas)
            .HasForeignKey(fl => fl.FacturaId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<FacturaLinea>()
            .HasOne(fl => fl.ConceptoCobro)
            .WithMany()
            .HasForeignKey(fl => fl.ConceptoCobroId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<FacturaLinea>()
            .HasIndex(fl => new { fl.FacturaId, fl.ConceptoCobroId });

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
            .HasForeignKey(p => p.FacturaId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Pago>()
            .HasIndex(p => p.IdempotencyKey)
            .IsUnique();

        modelBuilder.Entity<Pago>()
            .HasIndex(p => p.FacturaId);

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

        // MovimientoBancario configuration (pre-conciliación Afrime)
        modelBuilder.Entity<MovimientoBancario>()
            .Property(mb => mb.Tipo)
            .HasConversion<int>();

        modelBuilder.Entity<MovimientoBancario>()
            .Property(mb => mb.Estatus)
            .HasConversion<int>();


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

        // ReglaColegiatura configuration
        modelBuilder.Entity<ReglaColegiatura>()
            .HasOne(r => r.CicloEscolar)
            .WithMany()
            .HasForeignKey(r => r.CicloId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ReglaColegiatura>()
            .HasOne(r => r.Grupo)
            .WithMany()
            .HasForeignKey(r => r.GrupoId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<ReglaColegiatura>()
            .HasOne(r => r.ConceptoCobro)
            .WithMany()
            .HasForeignKey(r => r.ConceptoCobroId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ReglaColegiatura>()
            .Property(r => r.Turno)
            .HasMaxLength(50);

        modelBuilder.Entity<ReglaColegiatura>()
            .Property(r => r.MontoBase)
            .HasPrecision(18, 2);

        modelBuilder.Entity<ReglaColegiatura>()
            .HasIndex(r => new { r.CicloId, r.GrupoId, r.Grado, r.Turno, r.ConceptoCobroId })
            .IsUnique()
            .HasDatabaseName("IX_ReglaColegiatura_Unique");

        // BecaAlumno configuration
        modelBuilder.Entity<BecaAlumno>()
            .HasOne(b => b.Alumno)
            .WithMany()
            .HasForeignKey(b => b.AlumnoId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<BecaAlumno>()
            .HasOne(b => b.Ciclo)
            .WithMany()
            .HasForeignKey(b => b.CicloId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<BecaAlumno>()
            .Property(b => b.Tipo)
            .HasConversion<string>();

        modelBuilder.Entity<BecaAlumno>()
            .Property(b => b.Valor)
            .HasPrecision(18, 2);

        modelBuilder.Entity<BecaAlumno>()
            .HasIndex(b => new { b.AlumnoId, b.CicloId })
            .IsUnique();

        // ReglaRecargo configuration
        modelBuilder.Entity<ReglaRecargo>()
            .HasOne(r => r.CicloEscolar)
            .WithMany()
            .HasForeignKey(r => r.CicloId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ReglaRecargo>()
            .HasOne(r => r.ConceptoCobro)
            .WithMany()
            .HasForeignKey(r => r.ConceptoCobroId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ReglaRecargo>()
            .Property(r => r.Porcentaje)
            .HasPrecision(18, 4);

        modelBuilder.Entity<ReglaRecargo>()
            .HasIndex(r => r.CicloId)
            .IsUnique();

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

        // Grupo → Salon relationship
        modelBuilder.Entity<Grupo>()
            .HasOne(g => g.Salon)
            .WithMany()
            .HasForeignKey(g => g.SalonId)
            .OnDelete(DeleteBehavior.SetNull);

        // Grupo → DocenteTitular relationship
        modelBuilder.Entity<Grupo>()
            .HasOne(g => g.DocenteTitular)
            .WithMany()
            .HasForeignKey(g => g.DocenteTitularId)
            .OnDelete(DeleteBehavior.SetNull);

        // ReceptorFiscal configuration
        modelBuilder.Entity<ReceptorFiscal>()
            .HasOne(rf => rf.Alumno)
            .WithOne(a => a.ReceptorFiscal)
            .HasForeignKey<ReceptorFiscal>(rf => rf.AlumnoId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ReceptorFiscal>()
            .HasIndex(rf => rf.Rfc)
            .IsUnique();

        // FacturaFiscal configuration
        modelBuilder.Entity<FacturaFiscal>()
            .HasKey(ff => ff.FacturaId);

        modelBuilder.Entity<FacturaFiscal>()
            .HasOne(ff => ff.Factura)
            .WithOne(f => f.FacturaFiscal)
            .HasForeignKey<FacturaFiscal>(ff => ff.FacturaId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<FacturaFiscal>()
            .HasIndex(ff => ff.CfdiUuid)
            .IsUnique();
    }
}
