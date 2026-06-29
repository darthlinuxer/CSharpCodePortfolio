
namespace CSharpCodePortfolio.Tutorials.Tutorial30.Application.Commands;

/// <summary>
/// DTO returned by the command service after a registration command succeeds.
/// </summary>
/// <remarks>
/// <c>PhoneNumber</c> is exposed as the domain <see cref="Option{T}"/> at the
/// application layer so internal application services can compose with the
/// domain shape. The HTTP response adapter (<c>RegistrationEndpoint</c>)
/// closes the <c>Option</c> at the wire boundary by mapping it to
/// <c>string?</c> — <c>Option</c> never reaches the JSON payload.
/// </remarks>
public sealed record RegisteredUserDto(
    Guid Id,
    string Name,
    string Email,
    Option<PhoneNumber> PhoneNumber);