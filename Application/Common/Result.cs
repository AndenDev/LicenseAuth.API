
using Application.Common;
using System.Net;

namespace LicenseAuth.Application.Common
{
    public class Result<T>
    {
        public bool Success { get; protected set; }

        public T? Data { get; protected set; } = default;

        public string SuccessMessage { get; protected set; } = "";

        public HttpStatusCode StatusCode { get; protected set; }

        public ErrorDetails? Error { get; protected set; }

        protected Result(bool success, T? data, HttpStatusCode statusCode, ErrorDetails? error = null)
        {
            Success = success;
            Data = data;
            StatusCode = statusCode;
            Error = error;
        }

        public static Result<T> CreateSuccess(T data, string successMessage = "", HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            return new Result<T>(true, data, statusCode)
            {
                SuccessMessage = successMessage
            };
        }

        public static Result<T> CreateSuccess(string successMessage = "", HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            return new Result<T>(true, default, statusCode)
            {
                SuccessMessage = successMessage
            };
        }

        public static Result<T> CreateError(ErrorDetails? error, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        {
            return new Result<T>(false, default, statusCode, error);
        }

        public static Result<T> CreateError(T data, ErrorDetails? error, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        {
            return new Result<T>(false, data, statusCode, error);
        }
    }
}
