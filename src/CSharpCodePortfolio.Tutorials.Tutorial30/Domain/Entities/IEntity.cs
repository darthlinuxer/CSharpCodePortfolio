using LanguageExt;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Domain;

public interface IEntity
{
    Option<Timestamp> CreatedAt { get; }
    Option<UserAccount> CreatedBy { get; }
    Option<Timestamp> LastModified { get; }
    Option<UserAccount> LastModifiedBy { get; }
    Seq<IDomainEvent> DomainEvents { get; }
}

public interface IEntity<out TId> : IEntity
{
    TId Id { get; }
}
