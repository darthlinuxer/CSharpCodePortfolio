using LanguageExt;
using static LanguageExt.Prelude;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Common.Functional;

/// <summary>
/// Small interop helpers for boundaries that must translate between Option and nullable shapes.
/// </summary>
internal static class OptionExtensions
{
    /// <summary>
    /// Converts nullable materialized state into explicit domain optionality.
    /// </summary>
    internal static Option<T> ToOption<T>(this T? value)
        where T : class
    {
        return value is null ? None : Some(value);
    }

    /// <summary>
    /// Converts explicit optionality into the nullable shape expected by EF Core or HTTP.
    /// </summary>
    internal static T? ToNullable<T>(this Option<T> option)
        where T : class
    {
        foreach (var value in option)
        {
            return value;
        }

        return null;
    }
}
