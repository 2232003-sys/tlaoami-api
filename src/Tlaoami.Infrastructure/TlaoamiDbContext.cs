using Microsoft.EntityFrameworkCore;
using Tlaoami.Domain.Entities;
using Tlaoami.Domain.Enums;

public class TlaoamiDbContext : DbContext
{
    public TlaoamiDbContext(DbContextOptions<TlaoamiDbContext> options) : base(options)
    {
    }

    public DbSet<Alumno> Alumnos { get; set; }
    public DbSet<Factura> Facturas { get; set; }
    public DbSet<Pago> Pagos { get; set; }

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
    }
}
