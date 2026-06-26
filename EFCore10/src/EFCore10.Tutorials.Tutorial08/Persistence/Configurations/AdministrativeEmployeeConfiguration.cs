using EFCore10.Tutorials.Tutorial08.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFCore10.Tutorials.Tutorial08.Persistence;

internal sealed class AdministrativeEmployeeConfiguration : IEntityTypeConfiguration<AdministrativeEmployee>
{
    /// <summary>
    /// Configures administrative employee columns inside the Employee TPH table.
    /// </summary>
    public void Configure(EntityTypeBuilder<AdministrativeEmployee> administrativeEmployee)
    {
        // This type shares the Employees table with Professor through TPH.
        // Role only exists for the administrative branch, so it is nullable in
        // the physical table but still a value object in the domain model.
        administrativeEmployee.Property(value => value.Role)
            .HasConversion(value => value.Value, value => StaffRole.FromStorage(value))
            .HasMaxLength(StaffRole.MaxLength);
    }
}
