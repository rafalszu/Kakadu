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
    // public static class ExceptionMiddlewareExtensions
    // {
    //     public static void ConfigureExceptionHandler(this IApplicationBuilder app, ILogger<Startup> logger)
    //     {
    //         app.UseExceptionHandler(appError =>
    //         {
    //             appError.Run(async context =>
    //             {
    //                 context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
    //                 context.Response.ContentType = "application/json";
 
    //                 var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
    //                 if(contextFeature != null)
    //                 { 
    //                     logger.LogError($"Exception occurred: {contextFeature.Error?.Message}");
    //                     logger.LogDebug(contextFeature.Error.ToString());
                        
    //                     int statusCode = context.Response.StatusCode;
    //                     if(contextFeature.Error is HttpResponseException httpResponseException)
    //                     {
    //                         statusCode = ((HttpResponseException)httpResponseException).StatusCode;
    //                         context.Response.StatusCode = statusCode;
    //                     }

    //                     await context.Response.WriteAsync(new ApiError
    //                     {
    //                         StatusCode = statusCode,
    //                         Message = contextFeature.Error?.Message ?? "Internal server error"
    //                     }.ToString());
    //                 }
    //             });
    //         });
    //     }
    // }
}