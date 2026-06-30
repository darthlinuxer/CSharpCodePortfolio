using LanguageExt;
using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Events;
using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.ValueObjects;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Entities;

public interface IEntity<out TId>
{
    TId Id { get; }
    Option<Timestamp> CreatedAt { get; }
    Option<Timestamp> LastModified { get; }
}

public interface IAggregate
{
    bool HasDomainEvents { get; }
    Seq<IDomainEvent> RecordedDomainEvents { get; }
    void ClearDomainEvents();
}

public interface IAggregate<TAggregate, out TId> : IEntity<TId>, IAggregate
{
    Seq<AbstractDomainEvent<TAggregate>> DomainEvents { get; }
}
