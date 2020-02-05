using System;
using System.Threading.Tasks;
using Kakadu.ActionApi.Models;
using Kakadu.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Kakadu.ActionApi.Middleware
{
    public class HttpExceptionHandler
    {
        private readonly RequestDelegate request;
        private readonly ILogger<HttpExceptionHandler> logger;

        public HttpExceptionHandler(RequestDelegate pipeline, ILogger<HttpExceptionHandler> logger)
        {
            this.request = pipeline;
            this.logger = logger;
        }

        public Task Invoke(HttpContext context) => this.InvokeAsync(context);

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
                if(exception is ApiException rex)
                {
                    statusCode = rex.StatusCode;
                    message = rex.Content;
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