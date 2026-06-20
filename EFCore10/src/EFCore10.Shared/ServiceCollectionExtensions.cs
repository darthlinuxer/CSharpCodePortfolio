using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EFCore10.Shared;

/// <summary>
/// Shared service registration helpers for EF Core tutorials.
/// </summary>
public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers a DbContext configured with the SQLite provider.
        /// </summary>
        public IServiceCollection AddSqliteDbContext<TContext>(string connectionString)
            where TContext : DbContext
        {
            ArgumentNullException.ThrowIfNull(services);

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("The SQLite connection string cannot be empty.", nameof(connectionString));

            return services.AddDbContext<TContext>(options => options.UseSqlite(connectionString));
        }

        /// <summary>
        /// Registers a pooled DbContext factory configured with the SQLite provider.
        /// </summary>
        public IServiceCollection AddSqlitePooledDbContextFactory<TContext>(
            string connectionString,
            int poolSize = 1024)
            where TContext : DbContext
        {
            ArgumentNullException.ThrowIfNull(services);

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("The SQLite connection string cannot be empty.", nameof(connectionString));

            if (poolSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(poolSize), poolSize, "The DbContext pool size must be greater than zero.");

            return services.AddPooledDbContextFactory<TContext>(
                options => options.UseSqlite(connectionString),
                poolSize);
        }
    }
}
