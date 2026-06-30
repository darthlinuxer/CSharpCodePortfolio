using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Errors;
using LanguageExt;
using static LanguageExt.Prelude;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Functional;

public static class FunctionalExtensions
{
    public static Option<string> ToNonBlankOption(this string? value) =>
        string.IsNullOrWhiteSpace(value) ? None : Some(value);

    public static Either<DomainError, Unit> Ensure(this bool condition, Func<DomainError> error)
    {
        ArgumentNullException.ThrowIfNull(error);

        return condition
            ? Right<DomainError, Unit>(default)
            : Left<DomainError, Unit>(error());
    }

    public static Either<Seq<DomainError>, Unit> EnsureSeq(this bool condition, Func<DomainError> error)
    {
        ArgumentNullException.ThrowIfNull(error);

        return condition
            ? Right<Seq<DomainError>, Unit>(default)
            : Left<Seq<DomainError>, Unit>(Seq1(error()));
    }

    public static Option<DomainError> ErrorOption<T>(this Either<DomainError, T> result) =>
        result.Match(Right: _ => None, Left: Some);

    public static Either<Seq<DomainError>, TResult> Combine<T1, T2, T3, TResult>(
        this (Either<DomainError, T1> First, Either<DomainError, T2> Second, Either<DomainError, T3> Third) results,
        Func<T1, T2, T3, TResult> project)
    {
        ArgumentNullException.ThrowIfNull(project);

        var errors = Seq(
                results.First.ErrorOption(),
                results.Second.ErrorOption(),
                results.Third.ErrorOption())
            .Bind(error => error.Match(Some: Seq1, None: Seq<DomainError>));

        var projected =
            from first in results.First
            from second in results.Second
            from third in results.Third
            select project(first, second, third);

        return projected.Match(
            Right: value => Right<Seq<DomainError>, TResult>(value),
            Left: error => Left<Seq<DomainError>, TResult>(errors.IsEmpty ? Seq1(error) : errors));
    }

    public static Either<Seq<DomainError>, Seq<T>> Collect<T>(
        this IEnumerable<Either<Seq<DomainError>, T>> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        var evaluated = results.ToArray();
        var errors = evaluated
            .SelectMany(result => result.Match(Right: _ => Enumerable.Empty<DomainError>(), Left: value => value))
            .ToSeq();

        return errors.IsEmpty
            ? Right<Seq<DomainError>, Seq<T>>(evaluated
                .SelectMany(result => result.Match(Right: value => Enumerable.Repeat(value, 1), Left: _ => Enumerable.Empty<T>()))
                .ToSeq())
            : Left<Seq<DomainError>, Seq<T>>(errors);
    }
}
