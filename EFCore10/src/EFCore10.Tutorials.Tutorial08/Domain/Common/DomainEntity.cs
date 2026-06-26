using System.ComponentModel.DataAnnotations.Schema;

namespace EFCore10.Tutorials.Tutorial08.Domain;

[NotMapped]
internal abstract class DomainEntity<TId>
    where TId : notnull
{
    protected DomainEntity()
    {
    }

    protected DomainEntity(TId id)
    {
        Id = id;
    }

    public TId Id { get; private set; } = default!;
}
