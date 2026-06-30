using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Errors;
using LanguageExt;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Persistence;

public interface ITutorial30UnitOfWork
{
    Task<Either<Seq<DomainError>, int>> CommitAsync(CancellationToken cancellationToken);
}
