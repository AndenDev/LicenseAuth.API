
using Application.Common;
using System.Net;

namespace LicenseAuth.Application.Common
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Result<T>
    {
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Result{T}"/> is success.
        /// </summary>
        /// <value>
        ///   <c>true</c> if success; otherwise, <c>false</c>.
        /// </value>
        public bool Success { get; protected set; }

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        public T? Data { get; protected set; } = default;

        /// <summary>
        /// Gets or sets the success message.
        /// </summary>
        /// <value>
        /// The success message.
        /// </value>
        public string SuccessMessage { get; protected set; } = "";

        /// <summary>
        /// Gets or sets the status code.
        /// </summary>
        /// <value>
        /// The status code.
        /// </value>
        public HttpStatusCode StatusCode { get; protected set; }

        /// <summary>
        /// Gets or sets the error.
        /// </summary>
        /// <value>
        /// The error.
        /// </value>
        public ErrorDetails? Error { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Result{T}"/> class.
        /// </summary>
        /// <param name="success">if set to <c>true</c> [success].</param>
        /// <param name="data">The data.</param>
        /// <param name="statusCode">The status code.</param>
        /// <param name="error">The error.</param>
        protected Result(bool success, T? data, HttpStatusCode statusCode, ErrorDetails? error = null)
        {
            Success = success;
            Data = data;
            StatusCode = statusCode;
            Error = error;
        }

        /// <summary>
        /// Creates the success.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="successMessage">The success message.</param>
        /// <param name="statusCode">The status code.</param>
        /// <returns></returns>
        public static Result<T> CreateSuccess(T data, string successMessage = "", HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            return new Result<T>(true, data, statusCode)
            {
                SuccessMessage = successMessage
            };
        }

        /// <summary>
        /// Creates the success.
        /// </summary>
        /// <param name="successMessage">The success message.</param>
        /// <param name="statusCode">The status code.</param>
        /// <returns></returns>
        public static Result<T> CreateSuccess(string successMessage = "", HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            return new Result<T>(true, default, statusCode)
            {
                SuccessMessage = successMessage
            };
        }

        /// <summary>
        /// Creates the error.
        /// </summary>
        /// <param name="error">The error.</param>
        /// <param name="statusCode">The status code.</param>
        /// <returns></returns>
        public static Result<T> CreateError(ErrorDetails? error, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        {
            return new Result<T>(false, default, statusCode, error);
        }

        /// <summary>
        /// Creates the error.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="error">The error.</param>
        /// <param name="statusCode">The status code.</param>
        /// <returns></returns>
        public static Result<T> CreateError(T data, ErrorDetails? error, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        {
            return new Result<T>(false, data, statusCode, error);
        }
    }
}
