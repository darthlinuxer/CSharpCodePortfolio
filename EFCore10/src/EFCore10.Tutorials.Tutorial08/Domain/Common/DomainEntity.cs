namespace EFCore10.Tutorials.Tutorial08.Domain.Common;

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
