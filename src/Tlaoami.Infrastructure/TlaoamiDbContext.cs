using Microsoft.EntityFrameworkCore;
using Tlaoami.Domain.Entities;

public class TlaoamiDbContext : DbContext
{
    public TlaoamiDbContext(DbContextOptions<TlaoamiDbContext> options) : base(options)
    {
    }

    public DbSet<Alumno> Alumnos { get; set; }
    public DbSet<Factura> Facturas { get; set; }
    public DbSet<Pago> Pagos { get; set; }
    public DbSet<PaymentIntent> PaymentIntents { get; set; }
    public DbSet<MovimientoBancario> MovimientosBancarios { get; set; }
    public DbSet<MovimientoConciliacion> MovimientosConciliacion { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Alumno configuration
        modelBuilder.Entity<Alumno>()
            .HasIndex(a => a.Email)
            .IsUnique();

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
            .HasOne(p => p.Factura)
            .WithMany(f => f.Pagos)
            .HasForeignKey(p => p.FacturaId);

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
    }
}
