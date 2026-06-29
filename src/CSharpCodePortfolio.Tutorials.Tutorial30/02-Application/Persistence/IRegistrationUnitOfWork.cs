using CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Common.Errors;
using LanguageExt;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Application.Persistence;

/// <summary>
/// Application transaction boundary backed by EF Core in infrastructure.
/// </summary>
public interface IRegistrationUnitOfWork
{
    /// <summary>
    /// Commits tracked changes and returns expected persistence conflicts as domain errors.
    /// </summary>
    Task<Either<Seq<DomainError>, int>> CommitAsync(CancellationToken cancellationToken);
}
