using LanguageExt;
using static LanguageExt.Prelude;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Common.Functional;

/// <summary>
/// Small interop helpers for boundaries that must translate between Option and nullable shapes.
/// </summary>
internal static class OptionExtensions
{
    /// <summary>
    /// Converts nullable value type state into explicit domain optionality.
    /// </summary>
    internal static Option<T> ToOption<T>(this T? value)
        where T : struct
    {
        return value.HasValue ? Some(value.Value) : None;
    }

    /// <summary>
    /// Converts explicit optionality into the nullable shape expected by EF Core or HTTP.
    /// </summary>
    internal static T? ToNullable<T>(this Option<T> option)
    {
        foreach (var value in option)
        {
            return value;
        }

        return default;
    }
}
