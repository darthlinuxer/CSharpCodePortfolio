using OpenIDAppRazor.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace OpenIDAppRazor.Context
{
    public class OpenIdDbContext : IdentityDbContext
    {
        public DbSet<UserModel> OAuthUsers { get; set; }
        public OpenIdDbContext(DbContextOptions<OpenIdDbContext> options) : base(options)
        {
        }      
    }
}