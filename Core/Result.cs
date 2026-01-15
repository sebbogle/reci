namespace Reci.Core;

public class Result<T>
{
    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public T? Value { get; }

    public string? ErrorMessage { get; }

    private Result(bool isSuccess, T? value, string? errorMessage)
    {
        IsSuccess = isSuccess;
        Value = value;
        ErrorMessage = errorMessage;
    }

    public static Result<T> Success(T value) => new(true, value, null);

    public static Result<T> Failure(string errorMessage) => new(false, default, errorMessage);

    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<string, TResult> onFailure)
    {
        return IsSuccess ? onSuccess(Value!) : onFailure(ErrorMessage!);
    }

    public Result<TNew> Map<TNew>(Func<T, TNew> mapper)
    {
        return IsSuccess
            ? Result<TNew>.Success(mapper(Value!))
            : Result<TNew>.Failure(ErrorMessage!);
    }

    // Monad: Bind (flatMap)
    public Result<TNew> Bind<TNew>(Func<T, Result<TNew>> binder)
    {
        return IsSuccess
            ? binder(Value!)
            : Result<TNew>.Failure(ErrorMessage!);
    }

    // Applicative
    public Result<TNew> Apply<TNew>(Result<Func<T, TNew>> resultFunc)
    {
        return resultFunc.IsSuccess && IsSuccess
            ? Result<TNew>.Success(resultFunc.Value!(Value!))
            : Result<TNew>.Failure(resultFunc.ErrorMessage ?? ErrorMessage!);
    }

    // Side effects
    public Result<T> OnSuccess(Action<T> action)
    {
        if (IsSuccess)
            action(Value!);
        return this;
    }

    public Result<T> OnFailure(Action<string> action)
    {
        if (IsFailure)
            action(ErrorMessage!);
        return this;
    }

    // Tap (for side effects without changing the result)
    public Result<T> Tap(Action<T> action)
    {
        if (IsSuccess)
            action(Value!);
        return this;
    }

    // Get value or default
    public T GetValueOrDefault(T defaultValue = default!) => IsSuccess ? Value! : defaultValue;

    public T GetValueOrDefault(Func<T> defaultValueFactory) => IsSuccess ? Value! : defaultValueFactory();

    // Throw on failure
    public T GetValueOrThrow() => IsSuccess ? Value! : throw new InvalidOperationException(ErrorMessage);

    // Implicit conversions
    public static implicit operator Result<T>(T value) => Success(value);
    public static implicit operator bool(Result<T> result) => result.IsSuccess;

    // Override equality
    public override bool Equals(object? obj)
    {
        if (obj is not Result<T> other)
            return false;

        if (IsSuccess != other.IsSuccess)
            return false;

        return IsSuccess
            ? EqualityComparer<T>.Default.Equals(Value, other.Value)
            : ErrorMessage == other.ErrorMessage;
    }

    public override int GetHashCode()
    {
        return IsSuccess
            ? HashCode.Combine(IsSuccess, Value)
            : HashCode.Combine(IsSuccess, ErrorMessage);
    }

    public override string ToString()
    {
        return IsSuccess
            ? $"Success({Value})"
            : $"Failure({ErrorMessage})";
    }
}

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? ErrorMessage { get; }

    public Exception? Exception { get; init; }

    private Result(bool isSuccess, string? errorMessage)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }

    public static Result Success() => new(true, null);
    public static Result Failure(string errorMessage) => new(false, errorMessage);

    public TResult Match<TResult>(Func<TResult> onSuccess, Func<string, TResult> onFailure)
    {
        return IsSuccess ? onSuccess() : onFailure(ErrorMessage!);
    }

    public void Match(Action onSuccess, Action<string> onFailure)
    {
        if (IsSuccess)
            onSuccess();
        else
            onFailure(ErrorMessage!);
    }

    // Map to Result<T>
    public Result<T> Map<T>(Func<T> mapper)
    {
        return IsSuccess
            ? Result<T>.Success(mapper())
            : Result<T>.Failure(ErrorMessage!);
    }

    // Bind
    public Result Bind(Func<Result> binder)
    {
        return IsSuccess ? binder() : this;
    }

    public Result<T> Bind<T>(Func<Result<T>> binder)
    {
        return IsSuccess ? binder() : Result<T>.Failure(ErrorMessage!);
    }

    // Side effects
    public Result OnSuccess(Action action)
    {
        if (IsSuccess)
            action();
        return this;
    }

    public Result OnFailure(Action<string> action)
    {
        if (IsFailure)
            action(ErrorMessage!);
        return this;
    }

    // Tap
    public Result Tap(Action action)
    {
        if (IsSuccess)
            action();
        return this;
    }

    // Throw on failure
    public void ThrowOnFailure()
    {
        if (IsFailure)
            throw new InvalidOperationException(ErrorMessage);
    }

    // Implicit conversions
    public static implicit operator bool(Result result) => result.IsSuccess;

    // Combine multiple results
    public static Result Combine(params Result[] results)
    {
        foreach (var result in results)
        {
            if (result.IsFailure)
                return result;
        }
        return Success();
    }

    public static Result Combine(IEnumerable<Result> results)
    {
        foreach (var result in results)
        {
            if (result.IsFailure)
                return result;
        }
        return Success();
    }

    // Override equality
    public override bool Equals(object? obj)
    {
        if (obj is not Result other)
            return false;

        if (IsSuccess != other.IsSuccess)
            return false;

        return IsSuccess || ErrorMessage == other.ErrorMessage;
    }

    public override int GetHashCode()
    {
        return IsSuccess
            ? HashCode.Combine(IsSuccess)
            : HashCode.Combine(IsSuccess, ErrorMessage);
    }

    public override string ToString()
    {
        return IsSuccess
            ? "Success"
            : $"Failure({ErrorMessage})";
    }
}