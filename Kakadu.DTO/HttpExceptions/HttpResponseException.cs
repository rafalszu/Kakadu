using System;
using System.Net;

namespace Kakadu.DTO.HttpExceptions
{
    public class HttpResponseException : Exception
    {
        public int StatusCode { get; private set; } = 500;

        public string Body { get; private set; }

        public HttpResponseException() {}

        public HttpResponseException(string message) => Body = message;

        public HttpResponseException(HttpStatusCode code, string message)
        {
            StatusCode = (int)code;
            Body = message;
        }
    }

    public class HttpUnauthorizedException : HttpResponseException
    {
        public HttpUnauthorizedException(string message) : base(HttpStatusCode.Unauthorized, message) {}
    }

    public class HttpNotFoundException : HttpResponseException
    {
        public HttpNotFoundException(string message) : base(HttpStatusCode.NotFound, message) {}
    }

    public class HttpBadRequestException : HttpResponseException
    {
        public HttpBadRequestException(string message) : base(HttpStatusCode.BadRequest, message) {}
    }
}