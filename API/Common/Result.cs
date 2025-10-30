namespace API.Common
{
    public class Result
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public string Error { get; }
        public List<string> Errors { get; }

        protected Result(bool isSuccess, string error, List<string>? errors = null)
        {
            if (isSuccess && !string.IsNullOrEmpty(error))
                throw new InvalidOperationException("Success result cannot have error message");

            if (!isSuccess && string.IsNullOrEmpty(error) && (errors == null || !errors.Any()))
                throw new InvalidOperationException("Failure result must have error message");

            IsSuccess = isSuccess;
            Error = error;
            Errors = errors ?? new List<string>();
        }

        public static Result Success() => new Result(true, string.Empty);

        public static Result Failure(string error) => new Result(false, error);

        public static Result Failure(List<string> errors) => new Result(false, string.Join("; ", errors), errors);

        public static Result<T> Success<T>(T value) => new Result<T>(value, true, string.Empty);

        public static Result<T> Failure<T>(string error) => new Result<T>(default!, false, error);

        public static Result<T> Failure<T>(List<string> errors) => new Result<T>(default!, false, string.Join("; ", errors), errors);
    }

    public class Result<T> : Result
    {
        public T Value { get; }

        protected internal Result(T value, bool isSuccess, string error, List<string>? errors = null)
            : base(isSuccess, error, errors)
        {
            Value = value;
        }
    }
}
