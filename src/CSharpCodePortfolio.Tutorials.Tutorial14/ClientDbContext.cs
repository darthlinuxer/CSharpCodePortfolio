using Microsoft.EntityFrameworkCore;

namespace CSharpCodePortfolio.Tutorials.Tutorial14;

internal sealed class ClientDbContext(DbContextOptions<ClientDbContext> options) : DbContext(options)
{
    public DbSet<Client> Clients => Set<Client>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Client>(client =>
        {
            client.HasKey(entity => entity.Id);
            client.Property(entity => entity.Name).IsRequired().HasMaxLength(80);
            client.Property(entity => entity.Email).IsRequired().HasMaxLength(120);
        });
    }
}
