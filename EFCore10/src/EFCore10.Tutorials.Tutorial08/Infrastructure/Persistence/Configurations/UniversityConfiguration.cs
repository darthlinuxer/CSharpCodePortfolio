using EFCore10.Tutorials.Tutorial08.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFCore10.Tutorials.Tutorial08.Infrastructure.Persistence;

internal sealed class UniversityConfiguration : IEntityTypeConfiguration<University>
{
    /// <summary>
    /// Configures university identity and owned campus collection.
    /// </summary>
    public void Configure(EntityTypeBuilder<University> university)
    {
        // University is the main aggregate root for this tutorial and is mapped
        // to a concrete table. Departments and employees are reached through it.
        university.ToTable(Tables.Universities)
            .HasTableMapping(TableMappingMetadata.ConcreteTable);
        university.Navigation(value => value.Campuses)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
        university.Navigation(value => value.Departments)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
        university.Navigation(value => value.Employees)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Aggregate IDs and names are value objects. EF stores only their scalar
        // values in the database.
        university.HasKey(value => value.Id);
        university.Property(value => value.Id)
            .HasConversion(value => value.Value, value => UniversityId.FromStorage(value).RequireValue())
            .ValueGeneratedNever();
        university.Property(value => value.Name)
            .HasConversion(value => value.Value, value => UniversityName.FromStorage(value).RequireValue())
            .HasMaxLength(UniversityName.MaxLength)
            .IsRequired();

        // Campuses are owned by University: no repository, no independent
        // lifecycle. OwnsMany still creates a table because collections need
        // rows, but the owner key is a shadow property.
        university.OwnsMany(value => value.Campuses, campus =>
        {
            campus.HasTableMapping(TableMappingMetadata.OwnsMany);
            campus.ToTable(Tables.UniversityCampuses);
            campus.WithOwner()
                .HasForeignKey(Columns.UniversityId);

            // Shadow owner FK: Campus should not expose UniversityId in the
            // domain because it cannot exist outside University anyway.
            campus.Property<UniversityId>(Columns.UniversityId)
                .HasConversion(value => value.Value, value => UniversityId.FromStorage(value).RequireValue());
            campus.Property(value => value.Id)
                .HasConversion(value => value.Value, value => CampusId.FromStorage(value).RequireValue())
                .ValueGeneratedNever();
            campus.HasKey(Columns.UniversityId, nameof(UniversityCampus.Id));
            campus.Property(value => value.Name)
                .HasConversion(value => value.Value, value => CampusName.FromStorage(value).RequireValue())
                .HasMaxLength(CampusName.MaxLength)
                .IsRequired();
            campus.Property(value => value.City)
                .HasConversion(value => value.Value, value => CityName.FromStorage(value).RequireValue())
                .HasMaxLength(CityName.MaxLength)
                .IsRequired();
        });
    }
}
