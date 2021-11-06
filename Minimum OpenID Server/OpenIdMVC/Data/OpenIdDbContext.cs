using OpenIDAppMVC.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace OpenIDAppMVC.Context
{
    public class OpenIdDbContext : IdentityDbContext
    {
        public DbSet<UserModel> OAuthUsers { get; set; }
        public OpenIdDbContext(DbContextOptions<OpenIdDbContext> options) : base(options)
        {
        }      
    }
}