namespace ManutencaoPreditiva.Domain.Common
{
    public class Result<T>
    {
        public bool IsSuccess { get; }
        public T Value { get; }
        public string ErrorMessage { get; }
        public ResultErrorType ErrorType { get; }

        protected Result(bool isSuccess, T value, string errorMessage, ResultErrorType errorType)
        {
            IsSuccess = isSuccess;
            Value = value;
            ErrorMessage = errorMessage;
            ErrorType = errorType;
        }

        public static Result<T> Success(T value) => new Result<T>(true, value, null, ResultErrorType.None);
        public static Result<T> Failure(string errorMessage, ResultErrorType errorType = ResultErrorType.Generic)
        {
            return new Result<T>(false, default, errorMessage, errorType);
        }
    }

    public enum ResultErrorType
    {
        None,
        Generic,
        NotFound,
        Conflict,
        Invalid
    }
}
