using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Identity.Domain.Aggregates.UserAccounts;
using LanguageExt;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Identity.Application.Commands;

/// <summary>
/// DTO returned by command services after UserAccount mutations succeed.
/// </summary>
public sealed record UserAccountDto(
    Guid Id,
    string Name,
    string Email,
    Option<string> PhoneNumber)
{
    public static UserAccountDto From(UserAccount account)
    {
        ArgumentNullException.ThrowIfNull(account);

        return new UserAccountDto(
            account.Id,
            account.Name.Value,
            account.Email.Value,
            account.PhoneNumber.Map(phone => phone.Value));
    }
}
