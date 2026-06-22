using EFCore10.Tutorials.Tutorial07.Models;
using Microsoft.EntityFrameworkCore;

namespace EFCore10.Tutorials.Tutorial07.Persistence;

public abstract class LearningCatalogContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<LearningResource> Resources => Set<LearningResource>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureCommonModel(modelBuilder);
        ConfigureInheritance(modelBuilder);
    }

    protected abstract void ConfigureInheritance(ModelBuilder modelBuilder);

    private static void ConfigureCommonModel(ModelBuilder modelBuilder)
    {
        var resource = modelBuilder.Entity<LearningResource>();

        resource.HasKey(item => item.Id);

        resource.Property(item => item.Id)
            .HasConversion(id => id.Value, value => ResourceId.From(value))
            .ValueGeneratedNever();

        resource.Property(item => item.Title)
            .HasConversion(title => title.Value, value => ResourceTitle.Create(value))
            .HasMaxLength(ResourceTitle.MaxLength)
            .IsRequired();

        resource.Property(item => item.Instructor)
            .HasConversion(instructor => instructor.Value, value => InstructorName.Create(value))
            .HasMaxLength(InstructorName.MaxLength)
            .IsRequired();

        resource.Property(item => item.Level)
            .HasConversion(level => level.Value, value => LearningLevel.Create(value))
            .HasMaxLength(32)
            .IsRequired();

        resource.Property(item => item.CreatedOnUtc)
            .IsRequired();

        resource.Ignore(item => item.IsPublished);
        resource.Ignore(item => item.ResourceKind);

        modelBuilder.Entity<ArticleResource>()
            .Property(item => item.WordCount)
            .HasConversion(wordCount => wordCount.Value, value => WordCount.From(value))
            .HasColumnName("WordCount")
            .IsRequired();

        modelBuilder.Entity<VideoResource>()
            .Property(item => item.Duration)
            .HasConversion(duration => duration.Minutes, value => VideoDuration.FromMinutes(value))
            .HasColumnName("DurationMinutes")
            .IsRequired();

        modelBuilder.Entity<LiveWorkshopResource>()
            .Property(item => item.SeatLimit)
            .HasConversion(seatLimit => seatLimit.Value, value => SeatLimit.From(value))
            .HasColumnName("SeatLimit")
            .IsRequired();
    }
}

public sealed class TphLearningCatalogContext(DbContextOptions<TphLearningCatalogContext> options)
    : LearningCatalogContext(options)
{
    protected override void ConfigureInheritance(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LearningResource>()
            .UseTphMappingStrategy()
            .ToTable("LearningResources")
            .HasDiscriminator<string>("ResourceType")
            .HasValue<ArticleResource>("Article")
            .HasValue<VideoResource>("Video")
            .HasValue<LiveWorkshopResource>("LiveWorkshop");
    }
}

public sealed class TptLearningCatalogContext(DbContextOptions<TptLearningCatalogContext> options)
    : LearningCatalogContext(options)
{
    protected override void ConfigureInheritance(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LearningResource>()
            .UseTptMappingStrategy()
            .ToTable("LearningResources");

        modelBuilder.Entity<ArticleResource>().ToTable("Articles");
        modelBuilder.Entity<VideoResource>().ToTable("Videos");
        modelBuilder.Entity<LiveWorkshopResource>().ToTable("LiveWorkshops");
    }
}

public sealed class TpcLearningCatalogContext(DbContextOptions<TpcLearningCatalogContext> options)
    : LearningCatalogContext(options)
{
    protected override void ConfigureInheritance(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LearningResource>()
            .UseTpcMappingStrategy();

        modelBuilder.Entity<ArticleResource>().ToTable("Articles");
        modelBuilder.Entity<VideoResource>().ToTable("Videos");
        modelBuilder.Entity<LiveWorkshopResource>().ToTable("LiveWorkshops");
    }
}
