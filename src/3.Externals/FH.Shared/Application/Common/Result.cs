namespace FH.Shared.Application.Common;

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? ErrorMessage { get; }
    public string? ErrorCode { get; }
    public int StatusCode { get; }

    protected Result(bool isSuccess, string? errorMessage = null, string? errorCode = null, int statusCode = 200)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        ErrorCode = errorCode;
        StatusCode = statusCode;
    }

    public static Result Success(int statusCode = 200) => new(true, statusCode: statusCode);
    
    public static Result Failure(string errorMessage, string? errorCode = null, int statusCode = 400) =>
        new(false, errorMessage, errorCode, statusCode);

    public static Result NotFound(string message = "Recurso no encontrado.") =>
        new(false, message, "NotFound", 404);

    public static Result Conflict(string message = "Conflicto con el recurso actual.") =>
        new(false, message, "Conflict", 409);

    public static Result UnprocessableEntity(string message = "La entidad no pudo ser procesada.") =>
        new(false, message, "UnprocessableEntity", 422);
}

public class Result<T> : Result
{
    private readonly T? _value;

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("No se puede acceder al valor de un resultado fallido.");

    public T? ValueOrDefault => _value;

    protected Result(T? value, bool isSuccess, string? errorMessage = null, string? errorCode = null, int statusCode = 200)
        : base(isSuccess, errorMessage, errorCode, statusCode)
    {
        _value = value;
    }

    public static Result<T> Success(T value, int statusCode = 200) =>
        new(value, true, statusCode: statusCode);

    public new static Result<T> Failure(string errorMessage, string? errorCode = null, int statusCode = 400) =>
        new(default, false, errorMessage, errorCode, statusCode);

    public new static Result<T> NotFound(string message = "Recurso no encontrado.") =>
        new(default, false, message, "NotFound", 404);

    public new static Result<T> Conflict(string message = "Conflicto con el recurso actual.") =>
        new(default, false, message, "Conflict", 409);

    public new static Result<T> UnprocessableEntity(string message = "La entidad no pudo ser procesada.") =>
        new(default, false, message, "UnprocessableEntity", 422);

    public static implicit operator Result<T>(T value) => Success(value);
}
