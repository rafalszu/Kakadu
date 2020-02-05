using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using AspNetCore.Proxy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Kakadu.ActionApi.Extensions;
using Kakadu.DTO;
using Microsoft.Extensions.Primitives;
using Kakadu.ActionApi.Interfaces;
using System.Threading;

[assembly: InternalsVisibleTo("Kakadu.ActionApi.Tests")]
namespace Kakadu.ActionApi.Controllers
{
    public abstract class BaseActionApiController : ControllerBase
    {
        private readonly ILogger<BaseActionApiController> _logger;
        private readonly IAnonymousServiceClient _serviceClient;

        public BaseActionApiController(ILogger<BaseActionApiController> logger, IAnonymousServiceClient serviceClient)
        {
            _logger = logger;
            _serviceClient = serviceClient;
        }

        public abstract KnownRouteDTO GetKnownRoute(ServiceDTO service, string relativePath, string action = "");

        [NonAction]
        public async Task ProxyCall(string serviceCode, string relativePath, CancellationToken cancellationToken, bool saveResponse = false)
        {
            if(string.IsNullOrWhiteSpace(serviceCode))
                throw new ArgumentNullException(serviceCode);
            if(string.IsNullOrWhiteSpace(relativePath))
                throw new Exception($"No service found by given code: '{serviceCode}'");

            var service = await _serviceClient.GetByCodeAsync(serviceCode, cancellationToken);

            var options = ProxyOptions.Instance
                .WithHttpClientName("KakaduVirtualizationClient")
                .WithShouldAddForwardedHeaders(true)
                .WithIntercept(async (context) => await InterceptKnownRouteAsync(context, relativePath, service))                
                //.WithBeforeSend
                .WithAfterReceive((context, response) => {
                    if(saveResponse) {
                        _logger.LogInformation($"Saving response from {relativePath}");

                        // TODO: try match already existing knownroute and update it just-received data
                        // (serviceModel.KnownRoutes ??= new List<KnownRouteModel>()).Add(response.ToKnownRoute());
                        // dataProvider.UpdateService(serviceModel);

                        _logger.LogInformation($"{service.Code} known routes updated");
                    }

                    return Task.CompletedTask;
                });

            var url = CombinePaths(service.Address.AbsoluteUri, relativePath);

            await this.ProxyAsync(url, options);
        }

        private async Task<bool> InterceptKnownRouteAsync(HttpContext context, string relativePath, ServiceDTO dto)
        {
            var knownRoute = GetKnownRoute(dto, relativePath, GetSoapActionHeader(Request));
            if(knownRoute != null)
            {
                _logger.LogInformation($"Found known route for '{relativePath}', intercepting http call");

                var knownResponse = knownRoute.Replies?.FirstOrDefault();
                if(knownResponse != null)
                {
                    _logger.LogInformation($"Found matching response for '{relativePath}' request");

                    if(knownResponse.Headers?.Any() ?? false)
                    {
                        foreach(var headerKey in knownResponse.Headers.Keys)
                            context.Response.Headers[headerKey] = knownResponse.Headers[headerKey];
                    }

                    context.Response.StatusCode = knownResponse.StatusCode;
                    context.Response.ContentType = knownResponse.ContentTypeString;
                    context.Response.ContentLength = knownResponse.ContentLength;
                    if(!string.IsNullOrWhiteSpace(knownResponse.ContentEncoding))
                        context.Response.Headers["Content-Encoding"] = knownResponse.ContentEncoding;

                    await context.Response.Body.WriteAsync(knownResponse.ContentRaw, 0, knownResponse.ContentRaw.Length);

                    return true;
                }
                
                // no replies stored, can passthrough the call?
                if(dto.UnkownRoutesPassthrough)
                {
                    _logger.LogInformation($"No responses found for '{relativePath}', passing http call through");
                    return false;
                }

                // intercept call, return 200 with no content
                _logger.LogInformation($"No respones found for '{relativePath}', can't pass through as configured; returning NoContent");
                context.Response.StatusCode = 200;
                return true;
            }
            
            // route not known, but dont intercept and play ball
            if(dto.UnkownRoutesPassthrough)
            {
                _logger.LogInformation($"No known routes found for '{relativePath}', passign http call through");
                return false;
            }

            // route not known, no passthrough, intercept with 404
            _logger.LogInformation($"No known routes found for '{relativePath}', can't pass through as configured; returning NotFound");
            context.Response.StatusCode = 404;
            return true;
        }

        private string CombinePaths(string s, string v)
        {
            if(string.IsNullOrWhiteSpace(s) || string.IsNullOrWhiteSpace(v))
                throw new Exception("Can't combine paths as at least one part is null or empty");

            return string.Format("{0}/{1}", s.TrimEnd('/'), v.TrimStart('/'));
        }

        private async Task<string> GetRequestBodyAsync(string httpRequestMethod, Stream httpRequestBody)
        {
            if(string.IsNullOrWhiteSpace(httpRequestMethod))
                throw new ArgumentNullException(nameof(httpRequestMethod));
            if(httpRequestBody == null)
                throw new ArgumentNullException(nameof(httpRequestBody));
            
            if (httpRequestMethod.Equals("GET", StringComparison.InvariantCultureIgnoreCase) ||
                httpRequestMethod.Equals("HEAD", StringComparison.InvariantCultureIgnoreCase) ||
                httpRequestMethod.Equals("DELETE", StringComparison.InvariantCultureIgnoreCase))
                return string.Empty;

            string body = string.Empty;

            using (StreamReader reader  = new StreamReader(
                stream: httpRequestBody,
                encoding: System.Text.Encoding.UTF8,
                detectEncodingFromByteOrderMarks: true,
                bufferSize: 1024, 
                leaveOpen: true))
            {
                body = await reader.ReadToEndAsync();
            }

            // Rewind, so the core is not lost when it looks the body for the request
            httpRequestBody.Position = 0;

            return body;
        }

        private string GetSoapActionHeader(HttpRequest req)
        {
            req.Headers.TryGetValue("SOAPAction", out var actionValues);
            if(StringValues.IsNullOrEmpty(actionValues))
                return string.Empty;

            string action = actionValues.ToString().Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
            if(string.IsNullOrWhiteSpace(action))
                return string.Empty;

            return action.Sanitize();
        }
    }        
}