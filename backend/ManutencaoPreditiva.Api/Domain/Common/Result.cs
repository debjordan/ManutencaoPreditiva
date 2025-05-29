using Microsoft.AspNetCore.Mvc;

namespace ManutencaoPreditiva.Api.Domain.Common;

public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? ErrorMessage { get; }
    public ResultErrorType ErrorType { get; }

    private Result(T value)
    {
        IsSuccess = true;
        Value = value;
        ErrorMessage = null;
        ErrorType = ResultErrorType.None;
    }

    private Result(string errorMessage, ResultErrorType errorType)
    {
        IsSuccess = false;
        Value = default;
        ErrorMessage = errorMessage;
        ErrorType = errorType;
    }

    public static Result<T> Success(T value) => new Result<T>(value);

    public static Result<T> Failure(string errorMessage, ResultErrorType errorType) =>
        new Result<T>(errorMessage, errorType);

    public enum ResultErrorType
    {
        None,
        NoContent,
        BadRequest,
        NotFound,
        ServiceUnavailable,
        Unauthorized
    }
}
