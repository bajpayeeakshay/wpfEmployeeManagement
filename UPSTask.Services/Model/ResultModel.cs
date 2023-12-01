using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace UPSTask.Services
{
    public interface IRequestResult
    {
        RequestError Error { get; }

        bool IsSuccess { get; }
    }

    public static class RequestResult
    {
        public static RequestResult<TResult> Success<TResult>(TResult result) => new RequestResult<TResult>(result);
        public static RequestResult<TResult> Fail<TResult>(RequestError error) => new RequestResult<TResult>(error);
    }

    public class RequestResult<TResult> : IRequestResult
    {
        private readonly TResult result;

        public RequestResult(TResult result)
        {
            this.result = result;
        }

        public RequestResult(RequestError error)
        {
            Error = error ?? throw new ArgumentNullException(nameof(error));
        }

        public RequestError Error { get; }

        public bool IsSuccess => Error == null;
        public bool IsError => !IsSuccess;

        public TResult GetResult()
        {
            if (IsError)
            {
                throw new RequestErrorException(Error);
            }
            return result;
        }

        public Task<RequestResult<TResult>> GetTask() => Task.FromResult(this);
    }

    public class RequestError
    {
        public RequestError(string errorVal)
        {
            error = errorVal;
        }

        public string error { get; } = string.Empty;

        public override string ToString()
        {
            return error;
        }
    }

    public class RequestErrorException : Exception
    {
        public RequestError Error { get; }

        public RequestErrorException(RequestError error)
        {
            Error = error ?? throw new ArgumentNullException(nameof(error));
        }

        public RequestErrorException(string message) : base(message)
        {
        }

        public RequestErrorException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected RequestErrorException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}

