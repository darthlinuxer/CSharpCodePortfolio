using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Errors;
using LanguageExt;
using Microsoft.EntityFrameworkCore;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Infrastructure.Persistence;

public interface IEfPersistenceErrorTranslator
{
    Task<Option<Seq<DomainError>>> TranslateAsync(
        DbUpdateException exception,
        Tutorial30DbContext dbContext,
        CancellationToken cancellationToken);
}
