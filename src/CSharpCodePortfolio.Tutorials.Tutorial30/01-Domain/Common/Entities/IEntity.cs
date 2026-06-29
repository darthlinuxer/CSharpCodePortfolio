using LanguageExt;
using CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Common.Events;
using CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Common.ValueObjects;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Common.Entities;

public interface IEntity
{
    Option<Timestamp> CreatedAt { get; }
    Option<Timestamp> LastModified { get; }
    Seq<IDomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
}

public interface IEntity<TActor> : IEntity
    where TActor : class
{
    Option<TActor> CreatedBy { get; }
    Option<TActor> LastModifiedBy { get; }
}

public interface IEntity<out TId, TActor> : IEntity<TActor>
    where TActor : class
{
    TId Id { get; }
}
