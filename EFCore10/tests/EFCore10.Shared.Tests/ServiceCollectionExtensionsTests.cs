using EFCore10.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EFCore10.Shared.Tests;

[TestClass]
public sealed class ServiceCollectionExtensionsTests
{
    [TestMethod]
    public void AddSqliteDbContextRegistersContextWithSqliteProvider()
    {
        var services = new ServiceCollection();

        services.AddSqliteDbContext<TestDbContext>("Data Source=:memory:");

        using var serviceProvider = services.BuildServiceProvider(validateScopes: true);
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TestDbContext>();

        Assert.AreEqual("Microsoft.EntityFrameworkCore.Sqlite", context.Database.ProviderName);
    }

    [TestMethod]
    public async Task AddSqlitePooledDbContextFactoryRegistersFactoryWithSqliteProvider()
    {
        var services = new ServiceCollection();

        services.AddSqlitePooledDbContextFactory<TestDbContext>("Data Source=:memory:", poolSize: 1);

        using var serviceProvider = services.BuildServiceProvider(validateScopes: true);
        var factory = serviceProvider.GetRequiredService<IDbContextFactory<TestDbContext>>();
        await using var context = await factory.CreateDbContextAsync();

        Assert.AreEqual("Microsoft.EntityFrameworkCore.Sqlite", context.Database.ProviderName);
    }

    [TestMethod]
    public void AddSqlitePooledDbContextFactoryRejectsInvalidPoolSize()
    {
        var services = new ServiceCollection();

        var exception = Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            () => services.AddSqlitePooledDbContextFactory<TestDbContext>("Data Source=:memory:", poolSize: 0));

        Assert.AreEqual("poolSize", exception.ParamName);
    }

    private sealed class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options);
}
