namespace EFCore10.Tutorials.Tutorial08.Domain;

internal sealed record DomainError(string Code, string Message);

internal sealed class Result
{
    private Result(IReadOnlyList<DomainError> errors) => Errors = errors;

    public IReadOnlyList<DomainError> Errors { get; }

    public bool IsSuccess => Errors.Count == 0;

    public bool IsFailure => !IsSuccess;

    public static Result Success() => new([]);

    public static Result Failure(params DomainError[] errors) => Failure((IEnumerable<DomainError>)errors);

    public static Result Failure(IEnumerable<DomainError> errors)
    {
        var failures = errors.Where(static error => error is not null).ToArray();

        if (failures is [])
            throw new ArgumentException("Failure result requires at least one error.", nameof(errors));

        return new Result(failures);
    }

    public void RequireSuccess()
    {
        if (IsFailure)
            throw new InvalidOperationException(FormatErrors(Errors));
    }

    internal static string FormatErrors(IEnumerable<DomainError> errors) =>
        string.Join("; ", errors.Select(static error => $"{error.Code}: {error.Message}"));
}

internal sealed class Result<T>
{
    private Result(T value)
    {
        Value = value;
        Errors = [];
    }

    private Result(IReadOnlyList<DomainError> errors) => Errors = errors;

    public T? Value { get; }

    public IReadOnlyList<DomainError> Errors { get; }

    public bool IsSuccess => Errors.Count == 0;

    public bool IsFailure => !IsSuccess;

    public static Result<T> Success(T value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return new Result<T>(value);
    }

    public static Result<T> Failure(params DomainError[] errors) => Failure((IEnumerable<DomainError>)errors);

    public static Result<T> Failure(IEnumerable<DomainError> errors)
    {
        var failures = errors.Where(static error => error is not null).ToArray();

        if (failures is [])
            throw new ArgumentException("Failure result requires at least one error.", nameof(errors));

        return new Result<T>(failures);
    }

    public T RequireValue() =>
        IsSuccess && Value is not null
            ? Value
            : throw new InvalidOperationException(Result.FormatErrors(Errors));
}
