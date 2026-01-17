
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Tlaoami.Infrastructure
{
    public class TlaoamiDbContextFactory : IDesignTimeDbContextFactory<TlaoamiDbContext>
    {
        public TlaoamiDbContext CreateDbContext(string[] args)
        {
            // Build configuration
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../Tlaoami.API"))
                .AddJsonFile("appsettings.json")
                .Build();

            // Get connection string
            var optionsBuilder = new DbContextOptionsBuilder<TlaoamiDbContext>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            optionsBuilder.UseSqlite(connectionString);

            return new TlaoamiDbContext(optionsBuilder.Options);
        }
    }
}
