using CSharpCodePortfolio.Tutorials.Tutorial30.Domain;
using LanguageExt.Common;

namespace CSharpCodePortfolio.Tutorials.Tutorial30;

/// <summary>
/// Runs small executable examples for the LanguageExt.Core types introduced by the tutorial.
/// </summary>
public static class LanguageExtConceptsDemo
{
    /// <summary>
    /// Demonstrates Option, Either, Fin, Try, Seq, Map, Bind, Match, and LINQ syntax.
    /// </summary>
    public static LanguageExtConceptsReport Run()
    {
        Option<string> absentEmail = None;
        Option<string> suppliedEmail = Some("ADA@EXAMPLE.COM");

        var normalizedEmail =
            suppliedEmail
                .Map(email => email.Trim().ToLowerInvariant())
                .Match(Some: email => email, None: () => "sem email");

        var emailLength =
            from email in Email.Create("ada@example.com")
            from normalized in Right<DomainError, string>(email.Value)
            select normalized.Length;

        var composed =
            Some(10)
                .Map(value => value + 1)
                .Bind(value => value > 10 ? Some(value) : None);

        Fin<string> persistenceResult = FinSucc("persistência representada como sucesso");
        Try<int> technicalBoundary = Try(() => int.Parse("123"));
        Seq<int> scores = Seq(1, 2, 3).Map(value => value * 2);

        return new LanguageExtConceptsReport(
            absentEmail.IsNone,
            normalizedEmail,
            emailLength.Match(Right: value => value, Left: _ => 0),
            persistenceResult.Match(Succ: value => value, Fail: error => error.Message),
            technicalBoundary().Match(Succ: value => value, Fail: _ => 0),
            scores.Sum(),
            composed.Match(Some: value => value, None: () => 0));
    }
}

/// <summary>
/// Captures the observable values produced by the concept demo.
/// </summary>
public sealed record LanguageExtConceptsReport(
    bool AbsentEmailIsNone,
    string NormalizedEmail,
    int EmailLength,
    string FinMessage,
    int TryValue,
    int SeqTotal,
    int ComposedLength);
