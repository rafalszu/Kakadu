using System;
using System.Threading.Tasks;
using Kakadu.DTO;
using Kakadu.DTO.HttpExceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Kakadu.Common.Middleware
{
    public class HttpResponseExceptionHandler
    {
        private readonly RequestDelegate request;
        private readonly ILogger<HttpResponseExceptionHandler> logger;

        public HttpResponseExceptionHandler(RequestDelegate pipeline, ILogger<HttpResponseExceptionHandler> logger)
        {
            this.request = pipeline;
            this.logger = logger;
        }

        public Task Invoke(HttpContext context) => this.InvokeAsync(context); // Stops VS from nagging about async method without ...Async suffix.

        async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await this.request(context);
            }
            catch (Exception exception)
            {
                logger.LogError($"Exception occurred: {exception.Message}");

                if(logger.IsEnabled(LogLevel.Debug))
                    logger.LogDebug(exception.ToString());

                int statusCode = 500;
                string message = exception.Message;
                if(exception is HttpResponseException rex)
                {
                    statusCode = rex.StatusCode;
                    message = rex.Body;
                }

                context.Response.StatusCode = statusCode;
                context.Response.ContentType = "application/json";
 
                await context.Response.WriteAsync(new ApiErrorDTO
                {
                    StatusCode = statusCode,
                    Message = message
                }.ToString()).ConfigureAwait(false);
            }
        }
    }
}