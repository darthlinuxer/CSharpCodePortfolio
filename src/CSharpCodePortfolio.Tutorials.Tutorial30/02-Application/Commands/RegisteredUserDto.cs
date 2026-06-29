using LanguageExt;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Application.Commands;

/// <summary>
/// DTO returned by the command service after a registration command succeeds.
/// </summary>
/// <remarks>
/// <c>PhoneNumber</c> is exposed as <see cref="Option{T}"/> on purpose here:
/// the application layer carries the domain's absence semantic forward as
/// far as the wire DTO so HTTP adapters can decide how to present it
/// (typically <c>string?</c>). It is NOT an <c>Option&lt;string&gt;</c> leak
/// — the inner type is the domain value object.
/// </remarks>
public sealed record RegisteredUserDto(
    Guid Id,
    string Name,
    string Email,
    Option<PhoneNumber> PhoneNumber);