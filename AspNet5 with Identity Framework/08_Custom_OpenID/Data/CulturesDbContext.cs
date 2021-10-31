using System.Collections.Generic;
using App.Models;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Math.EC.Rfc7748;

namespace App.Context
{
    public class CulturesDbContext : DbContext
    {
        public DbSet<Language> Languages { get; set; }
        public DbSet<StringResource> StringResources { get; set; }
        public CulturesDbContext(DbContextOptions<CulturesDbContext> options) : base(options)
        {
            List<Language> validCultures = new()
            {
                new Language { Name = "English-US", Culture = "en-US" },
                new Language { Name = "Portuguese-Brazil", Culture = "pt-BR" },
                new Language { Name = "Franch-France", Culture = "fr-Fr" },
                new Language { Name = "German-Germany", Culture = "de-DE" }
            };
            Languages.AddRange(validCultures);

            List<StringResource> resources = new()
            {
                new StringResource { LanguageId = 1, Key = "Hello World!", Value = "Hello World!" },
                new StringResource { LanguageId = 2, Key = "Hello World!", Value = "Olá Mundo!" },
                new StringResource { LanguageId = 3, Key = "Hello World!", Value = "Salut tout le monde!" },
                new StringResource { LanguageId = 4, Key = "Hello World!", Value = "Hallo Welt!" }
            };
            StringResources.AddRange(resources);
            SaveChanges();
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Language>().HasKey(x => x.Id);
            modelBuilder.Entity<Language>()
                        .HasMany(x=>x.StringResources)
                        .WithOne(x=>x.Language)
                        .HasForeignKey(x=>x.LanguageId);
                        
                        
            modelBuilder.Entity<StringResource>().HasKey(x => x.Id);
            
        }

    }

}