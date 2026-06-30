using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Errors;
using Microsoft.AspNetCore.Http;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Presentation.Http;

/// <summary>
/// Translates domain error categories into HTTP response metadata.
/// </summary>
public static class DomainErrorHttpMap
{
    private static readonly IReadOnlyDictionary<DomainErrorCategory, (int Status, string Title)> Table =
        new Dictionary<DomainErrorCategory, (int Status, string Title)>
        {
            [DomainErrorCategory.Validation] = (StatusCodes.Status400BadRequest, "Cadastro inválido."),
            [DomainErrorCategory.Conflict] = (StatusCodes.Status409Conflict, "Conflito de cadastro."),
            [DomainErrorCategory.NotFound] = (StatusCodes.Status404NotFound, "Recurso não encontrado."),
        };

    /// <summary>
    /// Resolves an HTTP status and problem title for a domain error category.
    /// </summary>
    public static (int Status, string Title) Resolve(DomainErrorCategory category) =>
        Table.TryGetValue(category, out var result)
            ? result
            : throw new InvalidOperationException($"No HTTP mapping registered for domain error category '{category.Value}'.");
}
