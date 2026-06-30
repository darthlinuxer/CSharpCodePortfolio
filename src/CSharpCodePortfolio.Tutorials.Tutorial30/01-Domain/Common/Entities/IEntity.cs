using LanguageExt;
using CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Common.Events;
using CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Common.ValueObjects;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Common.Entities;

public interface IEntity<T>
{
    Option<Timestamp> CreatedAt { get; }
    Option<Timestamp> LastModified { get; }
    Seq<AbstractDomainEvent<T>> DomainEvents { get; }
    void ClearDomainEvents();
}

public interface IEntity<T, out TId> : IEntity<T>
{
    TId Id { get; }
}
