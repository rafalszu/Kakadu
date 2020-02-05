using Kakadu.ConfigurationApi.Models;
using Kakadu.DTO.HttpExceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Kakadu.ConfigurationApi.Middleware
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
 
                await context.Response.WriteAsync(new ApiError
                {
                    StatusCode = statusCode,
                    Message = message
                }.ToString()).ConfigureAwait(false);
                
                context.Response.Headers.Clear();
            }
        }
    }
}