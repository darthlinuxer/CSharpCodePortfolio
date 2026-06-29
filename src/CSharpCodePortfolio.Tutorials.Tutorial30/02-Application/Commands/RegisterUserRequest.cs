namespace CSharpCodePortfolio.Tutorials.Tutorial30.Application.Commands;

/// <summary>
/// DTO received by the application service from HTTP, tests, or console scenarios.
/// </summary>
/// <remarks>
/// The legacy <c>Document</c> field has been retired: the abstract
/// registration flow does not know about PF (CPF) or PJ (CNPJ) — that
/// modelling lives in a dedicated bounded context that the user will tackle
/// in a future tutorial.
/// </remarks>
public sealed record RegisterUserRequest(
    string? Name,
    string? Email,
    string? PhoneNumber);