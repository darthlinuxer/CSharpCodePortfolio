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
    }
}
