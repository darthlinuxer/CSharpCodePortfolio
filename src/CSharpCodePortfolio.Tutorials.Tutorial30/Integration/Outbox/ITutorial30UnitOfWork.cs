using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Errors;
using LanguageExt;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Integration.Outbox;

/// <summary>
/// Tutorial transaction boundary shared by the bounded-context adapters.
/// </summary>
public interface ITutorial30UnitOfWork
{
    /// <summary>
    /// Commits tracked changes and returns expected persistence conflicts as domain errors.
    /// </summary>
    Task<Either<Seq<DomainError>, int>> CommitAsync(CancellationToken cancellationToken);
}
