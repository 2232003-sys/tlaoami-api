
using Microsoft.EntityFrameworkCore;
using Tlaoami.Domain.Entities;

public class TlaoamiDbContext : DbContext
{
    public TlaoamiDbContext(DbContextOptions<TlaoamiDbContext> options) : base(options)
    {
    }

    public DbSet<Alumno> Alumnos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
