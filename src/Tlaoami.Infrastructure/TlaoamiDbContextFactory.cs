using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using System.IO;

namespace Tlaoami.Infrastructure
{
    public class TlaoamiDbContextFactory : IDesignTimeDbContextFactory<TlaoamiDbContext>
    {
        public TlaoamiDbContext CreateDbContext(string[] args)
        {
            // Build configuration
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../Tlaoami.API"))
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{env}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var provider = configuration.GetValue<string>("DatabaseProvider") ?? "Sqlite";
            var optionsBuilder = new DbContextOptionsBuilder<TlaoamiDbContext>();

            if (string.Equals(provider, "Postgres", StringComparison.OrdinalIgnoreCase))
            {
                var pgConnection = configuration.GetConnectionString("PostgresConnection");
                optionsBuilder.UseNpgsql(pgConnection);
            }
            else
            {
                var sqliteConnection = configuration.GetConnectionString("DefaultConnection");
                optionsBuilder.UseSqlite(sqliteConnection);
            }

            return new TlaoamiDbContext(optionsBuilder.Options);
        }
    }
}
