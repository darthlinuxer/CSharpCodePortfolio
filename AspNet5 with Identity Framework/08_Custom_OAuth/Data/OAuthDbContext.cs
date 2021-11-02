using OAuthApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace OAuthApp.Context
{
    public class OAuthDbContext : IdentityDbContext
    {
        public DbSet<OAuthUser> OAuthUsers { get; set; }
        public OAuthDbContext(DbContextOptions<OAuthDbContext> options) : base(options)
        {
        }

        // protected override void OnModelCreating(ModelBuilder builder)
        // {
        //     base.OnModelCreating(builder);

        //     builder.Entity<OAuthUser>()
        //        .Property(p => p.Client_ID)
        //        .IsRequired();

        //     builder.Entity<OAuthUser>()
        //         .Property(p => p.Client_Secret)
        //         .IsRequired();
        // }
    }
}