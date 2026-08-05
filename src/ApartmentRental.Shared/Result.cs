namespace ApartmentRental.Shared;

// Operation outcome without exceptions for control flow. Services return
// Result/Result<T> for expected failures (not found, duplicate, etc.);
// exceptions stay reserved for truly unexpected errors.
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }
    public string? ErrorCode { get; }

    protected Result(bool isSuccess, string? error, string? errorCode)
    {
        if (isSuccess && error is not null)
        {
            throw new InvalidOperationException("A successful result cannot carry an error message.");
        }

        IsSuccess = isSuccess;
        Error = error;
        ErrorCode = errorCode;
    }

    public static Result Success() => new(true, null, null);
    public static Result Failure(string error, string errorCode = "BAD_REQUEST") => new(false, error, errorCode);

    public static Result<T> Success<T>(T value) => new(value, true, null, null);
    public static Result<T> Failure<T>(string error, string errorCode = "BAD_REQUEST") => new(default, false, error, errorCode);

    public static Result NotFound(string entityName, object key) =>
        Failure($"{entityName} with id '{key}' was not found.", "NOT_FOUND");

    public static Result<T> NotFound<T>(string entityName, object key) =>
        Failure<T>($"{entityName} with id '{key}' was not found.", "NOT_FOUND");
}

public class Result<T> : Result
{
    private readonly T? _value;

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access the value of a failed result.");

    internal Result(T? value, bool isSuccess, string? error, string? errorCode)
        : base(isSuccess, error, errorCode)
    {
        _value = value;
    }

    public static implicit operator Result<T>(T value) => Success(value);
}
